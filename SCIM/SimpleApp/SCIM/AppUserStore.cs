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
    private readonly IPatchCommandExecutor patchCommandExecutor;
    private readonly ILogger<AppUserStore> logger;

    public AppUserStore(
        AppDbContext ctx,
        IScimQueryBuilderFactory queryBuilderFactory,
        IPatchCommandExecutor patchCommandExecutor,
        ILogger<AppUserStore> logger)
    {
        this.ctx = ctx;
        this.queryBuilderFactory = queryBuilderFactory;
        this.patchCommandExecutor = patchCommandExecutor;
        this.logger = logger;
    }

    public async Task<IEnumerable<string>> Exists(IEnumerable<string> ids)
    {
        return await ctx.Users.Select(u => u.Id).Intersect(ids).ToListAsync();
    }

    public async Task<User> Add(User resource)
    {
        var user = MapScimUserToAppUser(resource, new AppUser());
        await ctx.Users.AddAsync(user);
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw new ScimStoreItemAlreadyExistException($"User with userName: {resource.UserName} already exists");
        }

        return MapAppUserToScimUser(user);
    }

    public async Task<User> GetById(string id, ResourceAttributeSet attributes)
    {
        var user = await FindUser(id);

        return MapAppUserToScimUser(user);
    }

    private async Task<AppUser> FindUser(string id)
    {
        AppUser? user = await ctx.Users
            .Include(u => u.Addresses)
            .Include(u => u.Phones)
            .Include(u => u.Emails)
            .SingleOrDefaultAsync(u => u.Id == id);

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

        IQueryable<AppUser> baseQuery = ctx.Users
            .Include(u => u.Addresses)
            .Include(u => u.Phones)
            .Include(u => u.Emails)
            .Include(u => u.Roles);

        IQueryable<AppUser> databaseQuery =
            queryBuilderFactory.CreateQueryBuilder<AppUser>(baseQuery)
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

        IQueryable<AppUser> sortedSet = ctx.Users
            .Include(u => u.Addresses)
            .Include(u => u.Phones)
            .Include(u => u.Roles)
            .OrderBy(u => u.Id);

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

    public async Task<User?> PartialUpdate(string resourceId, IEnumerable<PatchCommand> commands)
    {
        AppUser user = await FindUser(resourceId);

        foreach (PatchCommand replaceCmd in commands)
        {
            try
            {
                patchCommandExecutor.Execute(user, replaceCmd);
            }
            catch (ScimStorePatchException)
            {
                logger.LogError("Failed to patch user with id {id} with command {cmd}", resourceId, replaceCmd);
            }
        }

        await ctx.SaveChangesAsync();

        return null;
    }

    private static User MapAppUserToScimUser(AppUser user)
    {
        return new User
        {
            Id = user.Id,
            UserName = user.Username,
            Active = !user.IsDisabled,
            DisplayName = user.DisplayName,
            NickName = user.Nickname,
            ProfileUrl = user.ProfileUrl,
            Title = user.Title,
            Timezone = user.Timezone,
            PreferredLanguage = user.PreferredLanguage,
            UserType = user.UserType,
            Addresses = MapAppAddressToAddress(user),
            Emails = MapAppEmailToEmail(user),
            PhoneNumbers = MapAppPhonenumbersToPhoneNumers(user),
            Locale = user.Locale,
            Name = new Name()
            {
                Formatted = user.Formatted,
                FamilyName = user.LastName,
                GivenName = user.FirstName,
                MiddleName = user.MiddleName,
                HonorificSuffix = user.HonorificSuffix,
                HonorificPrefix = user.HonorificPrefix
            },
            Extensions = new Dictionary<string, ResourceExtension>()
            {
                [ScimSchemas.EnterpriseUser] = new EnterpriseUser()
                {
                    Department = user.Department,
                    EmployeeNumber = user.EmployeeNumber,
                    Organization = user.Organization,
                    Division = user.Division,
                    CostCenter = user.CostCenter
                }
            }
        };
    }

    private static IEnumerable<Email> MapAppEmailToEmail(AppUser user)
    {
        if (user.Emails == null) return [];

        return user.Emails.Select(e =>
            new Email
            {
                Display = e.Value, Primary = e.Primary, Value = e.Value, Type = e.Type
            });
    }

    private static IEnumerable<Address> MapAppAddressToAddress(AppUser user)
    {
        if (user.Addresses == null) return [];

        return user.Addresses.Select(a => new Address
        {
            Formatted = a.Formatted,
            Primary = a.Primary,
            Type = a.Type,
            StreetAddress = a.StreetAddress,
            Locality = a.Locality,
            Region = a.Region,
            PostalCode = a.PostalCode,
            Country = a.Country
        });
    }

    private static IEnumerable<PhoneNumber> MapAppPhonenumbersToPhoneNumers(AppUser user)
    {
        return user.Phones == null ? [] :
            user.Phones.Select(p => new PhoneNumber
            {
                Primary = p.Primary,
                Type = p.Type,
                Value = p.Value
            });
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

        user.Username = resource.UserName ?? primaryEmail;
        user.Locale = resource.Locale ?? user.Locale;
        user.FirstName = resource.Name?.GivenName ?? user.FirstName;
        user.LastName = resource.Name?.FamilyName ?? user.LastName;
        user.Department = enterpriseUser?.Department ?? user.Department;
        user.NormalizedUsername = user.Username?.ToUpper();
        user.Nickname = resource.NickName ?? user.Nickname;
        user.Title = resource.Title ?? user.Title;
        user.DisplayName = resource.DisplayName ?? user.DisplayName;
        user.Timezone = resource.Timezone ?? user.Timezone;
        user.ProfileUrl = resource.ProfileUrl ?? user.ProfileUrl;
        user.Formatted = resource.Name?.Formatted ?? user.Formatted;
        user.MiddleName = resource.Name?.MiddleName ?? user.MiddleName;
        user.HonorificSuffix = resource.Name?.HonorificSuffix ?? user.HonorificSuffix;
        user.HonorificPrefix = resource.Name?.HonorificPrefix ?? user.HonorificPrefix;
        user.IsDisabled = !resource.Active ?? user.IsDisabled;
        user.PreferredLanguage = resource.PreferredLanguage ?? user.PreferredLanguage;
        user.EmployeeNumber = enterpriseUser?.EmployeeNumber ?? user.EmployeeNumber;
        user.Organization = enterpriseUser?.Organization ?? user.Organization;
        user.Division = enterpriseUser?.Division ?? user.Division;
        user.CostCenter = enterpriseUser?.CostCenter ?? user.CostCenter;
        user.UserType = resource.UserType;

        MapEmailsToAppEmails(resource, user);
        MapAddressToAppAddress(resource, user);
        MapPhoneNumbersToAppPhoneNumbers(resource, user);
        MapRolesToAppRoles(resource, user);
        return user;
    }

    private static void MapEmailsToAppEmails(User resource, AppUser user)
    {
        if (resource.Emails == null) return;

        user.Emails = resource.Emails.Select(e => new AppEmail
        {
            Value = e.Value,
            Primary = e.Primary,
            Type = e.Type
        }).ToArray();
    }

    private static void MapRolesToAppRoles(User resource, AppUser user)
    {
        if (resource.Roles != null)
        {
            user.Roles = resource.Roles.Select(r => new AppRole
            {
                Name = r.Value
            }).ToList();
        }
    }

    private static void MapPhoneNumbersToAppPhoneNumbers(User resource, AppUser user)
    {
        if (resource.PhoneNumbers != null)
        {
            user.Phones = resource.PhoneNumbers.Select(p => new AppPhoneNumber(p.Type)
            {
                Primary = p.Primary,
                Value = p.Value
            }).ToList();
        }
    }

    private static void MapAddressToAppAddress(User resource, AppUser user)
    {
        if (resource.Addresses == null) return;

        user.Addresses = resource.Addresses.Select(address => new AppAddress
        {
            Formatted = address.Formatted,
            StreetAddress = address.StreetAddress,
            Locality = address.Locality,
            Region = address.Region,
            PostalCode = address.PostalCode,
            Country = address.Country,
            Type = address.Type,
            Primary = address.Primary
        }).ToArray();
    }

    public async Task Delete(string id)
    {
        AppUser user = await FindUser(id);

        user.IsDisabled = true;

        await ctx.SaveChangesAsync();
    }
}