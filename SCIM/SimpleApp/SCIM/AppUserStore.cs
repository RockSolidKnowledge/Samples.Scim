using Microsoft.EntityFrameworkCore;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Exceptions;
using Rsk.AspNetCore.Scim.Filters;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;
using SimpleApp.Services;

namespace SimpleApp.SCIM;

public class AppUserStore : IScimStore<User>
{
    private readonly AppDbContext ctx;
    private readonly IScimQueryBuilderFactory queryBuilderFactory;

    public AppUserStore(
        AppDbContext ctx,
        IScimQueryBuilderFactory queryBuilderFactory
        )
    {
        this.ctx = ctx;
        this.queryBuilderFactory = queryBuilderFactory;
    }

    public async Task<IEnumerable<string>> Exists(IEnumerable<string> ids)
    {
        return await ctx.Users.Select(u => u.Id).Intersect(ids).ToListAsync();
    }

    public async Task<User> Add(User resource)
    {
        var user = MapScimUserToAppUser(resource, new AppUser());

        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        return MapAppUserToScimUser(user);
    }

    public async Task<User> GetById(string id, ResourceAttributeSet attributes)
    {
        var user = await FindUser(id);

        return MapAppUserToScimUser(user);
    }

    private async Task<AppUser> FindUser(string id)
    {
        AppUser? user = await ctx.Users.SingleOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new ScimStoreItemDoesNotExistException($"User with id '{id}' not found");
        }

        return user;
    }

    public async Task<ScimPageResults<User>> GetAll(IIndexResourceQuery query)
    {
        if (query.Filter.IsExternalIdEqualityExpression(out string? id))
        {
            return new ScimPageResults<User>(Enumerable.Empty<User>(), 0);
        }

        IQueryable<AppUser> databaseQuery =
            queryBuilderFactory.CreateQueryBuilder<AppUser>(ctx.Users)
                .Filter(query.Filter)
                .Build();

        IQueryable<AppUser> pageQuery = queryBuilderFactory.CreateQueryBuilder<AppUser>(databaseQuery)
            .Page(query.StartIndex, query.Count)
            .Sort(query.Sort.By, query.Sort.Direction)
            .Build();

        int totalCount = await databaseQuery.CountAsync();

        var matchingUsers = await pageQuery
            .AsAsyncEnumerable()
            .Select(MapAppUserToScimUser)
            .ToListAsync();

        return new ScimPageResults<User>(matchingUsers, totalCount);
    }

    public async Task<ScimCursorPageResults<User>> GetAll(ICursorResourceQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Cursor) && !Guid.TryParse(query.Cursor, out _))
        {
            throw new ScimStoreUnrecognizedCursorException("Cursor is not a valid GUID");
        }

        IQueryable<AppUser> sortedSet = ctx.Users.OrderBy(u => u.Id);

        IQueryable<AppUser> databaseQuery =
            queryBuilderFactory.CreateQueryBuilder(sortedSet)
                .Filter(query.Filter)
                .Build();

        IQueryable<AppUser> skipQuery = databaseQuery;

        if (!string.IsNullOrWhiteSpace(query.Cursor))
        {
            skipQuery = skipQuery.Where(q => string.Compare(q.Id, query.Cursor) > 0);
        }

        int totalCount = await databaseQuery.CountAsync();

        string? nextCursor = query.Count == int.MaxValue ? null :
            await skipQuery
                .Skip(query.Count + 1)
                .Take(1)
                .Select(sq => sq.Id)
                .FirstOrDefaultAsync();

        IQueryable<AppUser> pageQuery = queryBuilderFactory.CreateQueryBuilder(skipQuery)
            .Sort(query.Sort.By, query.Sort.Direction)
            .Build();

        var matchingUsers = await pageQuery
            .Take(query.Count)
            .AsAsyncEnumerable()
            .Select(MapAppUserToScimUser)
            .ToListAsync();

        string? previousCursor = string.IsNullOrWhiteSpace(query.Cursor) ? null : query.Cursor;

        return new ScimCursorPageResults<User>(matchingUsers, totalCount, nextCursor, previousCursor);
    }

    public async Task<User> Update(User resource)
    {
        AppUser user = await FindUser(resource.Id);

        MapScimUserToAppUser(resource, user);

        await ctx.SaveChangesAsync();

        return MapAppUserToScimUser(user);
    }

    private Dictionary<string, Action<AppUser, object>> replaceMethods =
        new()
        {
            [$"{ScimSchemas.User}:name"] = (source, value) =>
            {
                Name? name = value as Name;
                source.FirstName = name?.GivenName;
                source.LastName = name?.FamilyName;
            },
            [$"{ScimSchemas.User}:name.givenName"] = (source, value) => source.FirstName = (string)value,
            [$"{ScimSchemas.User}:name.familyName"] = (source, value) => source.LastName = (string)value,
            [$"{ScimSchemas.User}:active"] = (source, value) => source.IsDisabled = !(bool)value,
            [$"{ScimSchemas.User}:locale"] = (source, value) => source.Locale = (string)value,
            [$"{ScimSchemas.EnterpriseUser}:department"] = (source, value) => source.Department = (string)value,
        };


    public async Task<User?> PartialUpdate(string resourceId, IEnumerable<PatchCommand> updates)
    {
        AppUser user = await FindUser(resourceId);

        if (!updates.All(pc => pc.Operation == PatchOperation.Replace))
        {
            throw new ScimStoreException("Only Replace patch command supported for Users");
        }

        foreach (PatchCommand replaceCmd in updates)
        {
            if (replaceCmd.Path != null)
            {
                if (replaceMethods.TryGetValue(replaceCmd.Path.ToString(), out Action<AppUser, object>? replaceAction))
                {
                    replaceAction!(user, replaceCmd.Value);
                }
            }
            else
            {
                PatchFullUser(replaceCmd, user);
            }
        }

        await ctx.SaveChangesAsync();

        return null;
    }

    private static User MapAppUserToScimUser(AppUser user)
    {
        return new User()
        {
            Id = user.Id.ToString(),
            UserName = user.Username,
            Active = !user.IsDisabled,
            Emails = new Email[]
            {
                new Email() { Display = $"{user.FirstName} {user.LastName}", Primary = true, Value = user.Username }
            },
            Locale = user.Locale,
            Name = new Name()
            {
                Formatted = $"{user.FirstName} {user.LastName}",
                FamilyName = user.LastName,
                GivenName = user.FirstName,
            },
            Extensions = new Dictionary<string, ResourceExtension>()
            {
                [ScimSchemas.EnterpriseUser] = new EnterpriseUser() { Department = user.Department }
            }
        };
    }

    private static AppUser MapScimUserToAppUser(User resource, AppUser user)
    {
        string? primaryEmail = resource
            .Emails?
            .SingleOrDefault(e => e.Primary == true)
            ?.Value;

        if (primaryEmail == null)
        {
            primaryEmail = resource.UserName;
        }

        resource.Extensions.TryGetValue(ScimSchemas.EnterpriseUser, out ResourceExtension? resExt);
        var enterpriseUser = resExt as EnterpriseUser;

        user.Username = primaryEmail ?? user.Username;
        user.Locale = resource.Locale ?? user.Locale;
        user.FirstName = resource.Name?.GivenName ?? user.FirstName;
        user.LastName = resource.Name?.FamilyName ?? user.LastName;
        user.Department = enterpriseUser?.Department ?? user.Department;
        user.NormalizedUsername = user.Username?.ToUpper();

        user.IsDisabled = !resource?.Active ?? user.IsDisabled;
        return user;
    }

    private static void PatchFullUser(PatchCommand replaceCmd, AppUser user)
    {
        var source = (User)replaceCmd.Value;

        MapScimUserToAppUser(source, user);
    }

    public async Task Delete(string id)
    {
        AppUser user = await FindUser(id);

        ctx.Users.Remove(user);

        await ctx.SaveChangesAsync();
    }
}