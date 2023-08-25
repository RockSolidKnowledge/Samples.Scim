using SimpleApp.Services;

namespace SimpleApp.Models;

public class AccountsModel
{
    public AccountsModel()
    {
        AllUsers = new List<AppUserModel>();
        AllRoles = new List<AppRoleModel>();
    }
    public List<AppUserModel> AllUsers { get; set; }
    public List<AppRoleModel> AllRoles { get; set; }
}

public class AppRoleModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }

    public List<AppUserModel> Users { get; set; } = new List<AppUserModel>();
}

public class AppUserModel
{
    private readonly AppUser user;

    public AppUserModel(AppUser user )
    {
        this.user = user;
    }

    public string? Username => user.Username;
    public string Name => $"{user.FirstName} {user.LastName}";
    public string Id => user.Id.ToString();
    public string? Culture => user.Locale;
    public string IsActive => !user.IsDisabled ? "Enabled" : "Disabled";

}