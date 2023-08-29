using Microsoft.EntityFrameworkCore;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Exceptions;
using Rsk.AspNetCore.Scim.Filters;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Parsers;
using Rsk.AspNetCore.Scim.Stores;
using SimpleApp.Models;
using SimpleApp.Services;

namespace SimpleApp.SCIM;

public class AppRoleStore : IScimStore<Group>
{
    private readonly AppDbContext ctx;
    private readonly IScimQueryBuilderFactory queryBuilderFactory;

    public AppRoleStore(AppDbContext ctx , IScimQueryBuilderFactory queryBuilderFactory)
    {
        this.ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        this.queryBuilderFactory = queryBuilderFactory;
    }

    public async Task<IEnumerable<string>> Exists(IEnumerable<string> ids)
    {
        return await ctx.Roles.Select(u => u.Id).Intersect(ids).ToListAsync();
    }

    public async Task<Group> Add(Group resource)
    {
        var role = new AppRole();
        MapScimGroupToAppRole(resource, role);

        await ctx.Roles.AddAsync(role);

        await ctx.SaveChangesAsync();
        
        return MapAppRoleToScimGroup(role);
    }

    private static Group MapAppRoleToScimGroup(AppRole role)
    {
        return new Group()
        {
            Id = role.Id,
            DisplayName = role.Name,
            Members = role.Members.Select(u => new Member()
            {
                Value = u.Id,
                Display = $"{u.FirstName} {u.LastName}",
                Type = "User",
                ScimRef = $"/scim/users/{u.Id}" // TODO: This needs to change to be full URI
            }).ToList()
        };
    }
    
    private void MapScimGroupToAppRole(Group resource, AppRole appRole)
    {
        appRole.Name = resource.DisplayName ?? appRole.Name;

        if (resource.Members != null)
        {
            appRole.Members.Clear();

            foreach (var member in resource.Members)
            {
                AppUser user = new(member.Value);
                ctx.Users.Attach(user);
                appRole.Members.Add(user);
            }
        }
    }

  

    public async Task<Group> GetById(string id, ResourceAttributeSet attributeSet)
    {
        IQueryable<AppRole> source = ctx.Roles;

        var includeMembers = IncludeMembers(attributeSet);
                               
        source = includeMembers ? source.Include(ar => ar.Members) : source;

        AppRole role = await FindRole(source, id);
        Group group = MapAppRoleToScimGroup(role);

        return group;
    }

    private static bool IncludeMembers(ResourceAttributeSet attributeSet)
    {
        return attributeSet.IsInSet(groupMembers);
    }

    public async Task<ScimPageResults<Group>> GetAll(IResourceQuery query)
    {
        IQueryable<AppRole> source = ctx.Roles;

        var includeMembers = IncludeMembers(query.AttributeSet);
        
        source = includeMembers ? source.Include(ar => ar.Members) : source;
        
        IQueryable<AppRole> databaseQuery = 
            queryBuilderFactory.CreateQueryBuilder<AppRole>(source)
                .Filter(query.Filter)
                .Build();
            
        IQueryable<AppRole> pageQuery = queryBuilderFactory.CreateQueryBuilder<AppRole>(databaseQuery)
            .Page(query.StartIndex, query.Count)
            .Sort(query.Sort.By, query.Sort.Direction)
            .Build();
        
        int totalCount = await databaseQuery.CountAsync();

        var matchingGroups = await pageQuery
            .AsAsyncEnumerable()
            .Select(MapAppRoleToScimGroup)
            .ToListAsync();

        return new ScimPageResults<Group>(matchingGroups, totalCount);
    }

    public async Task<Group> Update(Group resource)
    {
        var role = await FindRole( ctx.Roles.Include(r=>r.Members), resource.Id);
        role.Members.Clear();
        
        MapScimGroupToAppRole(resource,role);

        await ctx.SaveChangesAsync();

        return MapAppRoleToScimGroup(role);
    }

    private async Task<AppRole> FindRole( IQueryable<AppRole> source ,  string id)
    {
        var role = await source.SingleOrDefaultAsync(u => u.Id == id);

        if (role == null)
        {
            throw new ScimStoreItemDoesNotExistException($"Role not found for resource '{id}'");
        }

        return role;
    }

    private static readonly AttributePathExpression groupMembers = new AttributePathExpression(ScimSchemas.Group, "members");
    private static readonly AttributePathExpression displayName = new AttributePathExpression(ScimSchemas.Group, "displayName");
    
    private static readonly PathExpression groupPathExpression = new PathExpression(groupMembers);
    private static readonly PathExpression displayNameExpression = new PathExpression(displayName);

    public async Task<Group?> PartialUpdate(string resourceId, IEnumerable<PatchCommand> updates)
    {
        AppRole role = await FindRoleNoMembers(resourceId);

        foreach (PatchCommand update in updates)
        {
            // Replace entire group 
            if (update.Path == null && update.Operation == PatchOperation.Replace)
            {
                MapScimGroupToAppRole((Group)update.Value, role);
            } // add or remove a member by value
            else if (update.Path != null && update.Path.Equals(groupPathExpression) )
            {
               if (update.Value is Member[] members)
               {
                   UpdateMembers(update, members, role);
               }
            } // Remove a member by query
            else if (update.Operation == PatchOperation.Remove &&
                     (update.Path?.PathElements[0] is ValuePathExpression filter ) &&
                     (filter.PathElements[0] == "members") )
            {
                var source = ctx.UserRoles.Where(r => r.AppRoleId== resourceId);
                IQueryable<AppUserRole> databaseQuery = 
                    queryBuilderFactory.CreateQueryBuilder<AppUserRole>(source)
                        .Filter(filter.ValueFilter)
                        .Build();

                // Find all members that match
                var toRemove = await databaseQuery.ToListAsync();
                
                ctx.RemoveRange(toRemove);
            }
            else if ( update.Operation == PatchOperation.Replace &&
                      update.Path != null &&
                      update.Path.Equals(displayNameExpression))
            {
                role.Name = (string)update.Value;
            }
        }

        await ctx.SaveChangesAsync();
        return null;
    }

    private void UpdateMembers(PatchCommand update, Member[] members, AppRole role)
    {
        Action<AppRole, string> updateRole =
            update.Operation == PatchOperation.Add
                ? AddUserToRole
                : RemoveUserFromRole;

        foreach (Member member in members)
        {
            updateRole(role, member.Value);
        }
    }

    private void AddUserToRole(AppRole role, string userId)
    {
        var user = ctx.Users.Single(u => u.Id == userId);
        role.Members.Add(user);
    }
    
    private void RemoveUserFromRole(AppRole role, string userId)
    {
        var user = ctx.Users.Single(u => u.Id == userId);
        role.Members.Remove(user);
    }

    public async Task Delete(string id)
    {
        AppRole role = await FindRoleNoMembers(id);

        if (role == null)
        {
            throw new ScimStoreItemAlreadyExistException($"Can not find group with id {id}");
        }
        
        ctx.Roles.Remove(role);
        await ctx.SaveChangesAsync();
    }
    
    private async Task<AppRole> FindRoleNoMembers(string id)
    {
        AppRole? role = await ctx.Roles.SingleOrDefaultAsync(u => u.Id == id);

        if (role == null)
        {
            throw new ScimStoreItemAlreadyExistException($"Can not find group with id {id}");
        }
        return role;
    }
}