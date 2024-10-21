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
            .Include(u => u.Phones)
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
            .Include(u => u.Address)
            .Include(u => u.Phones)
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
            .Include(u => u.Address)
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

    private Dictionary<string, Action<AppUser, object>> patchingMethods =
        new()
        {
            [$"{ScimSchemas.User}:name"] = (source, value) =>
            {
                Name? name = value as Name;
                source.FirstName = name?.GivenName;
                source.LastName = name?.FamilyName;
                source.Formatted = name?.Formatted;
                source.MiddleName = name?.MiddleName;
                source.HonorificSuffix = name?.HonorificSuffix;
                source.HonorificPrefix = name?.HonorificPrefix;
            },
            [$"{ScimSchemas.User}:displayName"] = (source, value) => source.DisplayName = (string)value,
            [$"{ScimSchemas.User}:title"] = (source, value) => source.Title = (string)value,
            [$"{ScimSchemas.User}:preferredLanguage"] = (source, value) => source.PreferredLanguage = (string)value,
            [$"{ScimSchemas.User}:userType"] = (source, value) => source.UserType = (string)value,
            [$"{ScimSchemas.User}:userName"] = (source, value) => source.Username = (string)value,
            [$"{ScimSchemas.User}:nickName"] = (source, value) => source.Nickname = (string)value,
            [$"{ScimSchemas.User}:timezone"] = (source, value) => source.Timezone = (string)value,
            [$"{ScimSchemas.User}:profileUrl"] = (source, value) => source.ProfileUrl = (string)value,
            [$"{ScimSchemas.User}:name.givenName"] = (source, value) => source.FirstName = (string)value,
            [$"{ScimSchemas.User}:name.familyName"] = (source, value) => source.LastName = (string)value,
            [$"{ScimSchemas.User}:name.formatted"] = (source, value) => source.Formatted = (string)value,
            [$"{ScimSchemas.User}:name.middleName"] = (source, value) => source.MiddleName = (string)value,
            [$"{ScimSchemas.User}:name.honorificPrefix"] = (source, value) => source.HonorificPrefix = (string)value,
            [$"{ScimSchemas.User}:name.honorificSuffix"] = (source, value) => source.HonorificSuffix = (string)value,
            [$"{ScimSchemas.User}:active"] = (source, value) => source.IsDisabled = !(bool)value,
            [$"{ScimSchemas.User}:locale"] = (source, value) => source.Locale = (string)value,
            [$"{ScimSchemas.EnterpriseUser}:department"] = (source, value) => source.Department = (string)value,
            [$"{ScimSchemas.EnterpriseUser}:employeeNumber"] = (source, value) => source.EmployeeNumber = (string)value,
            [$"{ScimSchemas.EnterpriseUser}:organization"] = (source, value) => source.Organization = (string)value,
            [$"{ScimSchemas.EnterpriseUser}:division"] = (source, value) => source.Division = (string)value,
            [$"{ScimSchemas.EnterpriseUser}:costCenter"] = (source, value) => source.CostCenter = (string)value,
            [$"{ScimSchemas.User}:addresses[{ScimSchemas.User}:type eq \"work\"].{ScimSchemas.User}:formatted"] = (source, value) =>
            {
                source.Address ??= new AppAddress();
                source.Address.Formatted = (string)value;
            },
            [$"{ScimSchemas.User}:addresses[{ScimSchemas.User}:type eq \"work\"].{ScimSchemas.User}:streetAddress"] = (source, value) =>
            {
                source.Address ??= new AppAddress();
                source.Address.StreetAddress = (string)value;
            },
            [$"{ScimSchemas.User}:addresses[{ScimSchemas.User}:type eq \"work\"].{ScimSchemas.User}:locality"] = (source, value) =>
            {
                source.Address ??= new AppAddress();
                source.Address.Locality = (string)value;
            },
            [$"{ScimSchemas.User}:addresses[{ScimSchemas.User}:type eq \"work\"].{ScimSchemas.User}:region"] = (source, value) =>
            {
                source.Address ??= new AppAddress();
                source.Address.Region = (string)value;
            },
            [$"{ScimSchemas.User}:addresses[{ScimSchemas.User}:type eq \"work\"].{ScimSchemas.User}:postalCode"] = (source, value) =>
            {
                source.Address ??= new AppAddress();
                source.Address.PostalCode = (string)value;
            },
            [$"{ScimSchemas.User}:addresses[{ScimSchemas.User}:type eq \"work\"].{ScimSchemas.User}:country"] = (source, value) =>
            {
                source.Address ??= new AppAddress();
                source.Address.Country = (string)value;
            },
            [$"{ScimSchemas.User}:phoneNumbers[{ScimSchemas.User}:type eq \"work\"].{ScimSchemas.User}:value"] = (source, value) =>
            {
                AppPhoneNumber number = GetOrCreatePhoneNumber(source, "work");
                number.Value = (string)value;
            },
            [$"{ScimSchemas.User}:phoneNumbers[{ScimSchemas.User}:type eq \"work\"].{ScimSchemas.User}:primary"] = (source, value) =>
            {
                SetPrimaryMobile(source, "work");
            },
            [$"{ScimSchemas.User}:phoneNumbers[{ScimSchemas.User}:type eq \"fax\"].{ScimSchemas.User}:value"] = (source, value) =>
            {
                AppPhoneNumber number = GetOrCreatePhoneNumber(source, "fax");
                number.Value = (string)value;
            },
            [$"{ScimSchemas.User}:phoneNumbers[{ScimSchemas.User}:type eq \"fax\"].{ScimSchemas.User}:primary"] = (source, value) =>
            {
                SetPrimaryMobile(source, "fax");
            },
            [$"{ScimSchemas.User}:phoneNumbers[{ScimSchemas.User}:type eq \"mobile\"].{ScimSchemas.User}:value"] = (source, value) =>
            {
                AppPhoneNumber number = GetOrCreatePhoneNumber(source, "mobile");
                number.Value = (string)value;
            },
            [$"{ScimSchemas.User}:phoneNumbers[{ScimSchemas.User}:type eq \"mobile\"].{ScimSchemas.User}:primary"] = (source, value) =>
            {
                SetPrimaryMobile(source, "mobile");
            },
            [$"{ScimSchemas.User}:emails[{ScimSchemas.User}:type eq \"work\"].{ScimSchemas.User}:value"] = (source, value) =>
            {
                source.Email = (string)value;
            }
    };

    private static AppPhoneNumber GetOrCreatePhoneNumber(AppUser source, string type)
    {
        AppPhoneNumber? phoneNumber;

        if (source.Phones == null)
        {
            phoneNumber = new AppPhoneNumber(type);
            source.Phones = new List<AppPhoneNumber> { phoneNumber };

            return phoneNumber;
        }

        phoneNumber = source.Phones.FirstOrDefault(p => p.Type == type);

        if (phoneNumber != null)
        {
            return phoneNumber;
        }

        phoneNumber = new AppPhoneNumber(type);

        source.Phones.Add(phoneNumber);

        return phoneNumber;
    }

    private static void SetPrimaryMobile(AppUser source, string type)
    {
        var number = GetOrCreatePhoneNumber(source, type);
        number.Primary = true;

        foreach (var appPhoneNumber in source.Phones.Where(p => p.Type != type))
        {
            appPhoneNumber.Primary = false;
        }
    }

    public async Task<User?> PartialUpdate(string resourceId, IEnumerable<PatchCommand> commands)
    {
        AppUser user = await FindUser(resourceId);

        foreach (PatchCommand replaceCmd in commands.Where(u => u.Operation != PatchOperation.Remove))
        {
            if (patchingMethods.TryGetValue(replaceCmd.Path.ToString(), out Action<AppUser, object>? replaceAction))
            {
                replaceAction(user, replaceCmd.Value);
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
            DisplayName = user.DisplayName,
            NickName = user.Nickname,
            ProfileUrl = user.ProfileUrl,
            Title = user.Title,
            Timezone = user.Timezone,
            PreferredLanguage = user.PreferredLanguage,
            UserType = user.UserType,
            Addresses = MapAppAddressToAddress(user),
            Emails = new Email[]
            {
                new Email() { Display = $"{user.FirstName} {user.LastName}", Primary = true, Value = user.Email, Type = "work"}
            },
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

    private static IEnumerable<Address> MapAppAddressToAddress(AppUser user)
    {
        return user.Address != null
            ? [ new Address
                {
                    Formatted = user.Address.Formatted,
                    Primary = true,
                    Type = "work",
                    StreetAddress = user.Address.StreetAddress,
                    Locality = user.Address.Locality,
                    Region = user.Address.Region,
                    PostalCode = user.Address.PostalCode,
                    Country = user.Address.Country
                }
            ]
            : [];
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
        user.Email = primaryEmail;
        user.EmployeeNumber = enterpriseUser?.EmployeeNumber ?? user.EmployeeNumber;
        user.Organization = enterpriseUser?.Organization ?? user.Organization;
        user.Division = enterpriseUser?.Division ?? user.Division;
        user.CostCenter = enterpriseUser?.CostCenter ?? user.CostCenter;
        user.UserType = resource.UserType;

        MapAddressToAppAddress(resource, user);
        MapPhoneNumbersToAppPhoneNumbers(resource, user);
        MapRolesToAppRoles(resource, user);
        return user;
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
        if (resource.Addresses != null)
        {
            var address = resource.Addresses.First();
            user.Address = new AppAddress
            {
                Formatted = address.Formatted,
                StreetAddress = address.StreetAddress,
                Locality = address.Locality,
                Region = address.Region,
                PostalCode = address.PostalCode,
                Country = address.Country
            };
        }
    }

    public async Task Delete(string id)
    {
        AppUser user = await FindUser(id);

        user.IsDisabled = true;

        await ctx.SaveChangesAsync();
    }
}