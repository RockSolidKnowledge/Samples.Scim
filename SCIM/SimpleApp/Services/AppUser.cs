namespace SimpleApp.Services;

public class AppUser
{
    public AppUser()
    {
        Id = Guid.NewGuid().ToString();
        Roles = new List<AppRole>();
    }

    public AppUser(string id) : this()
    {
        Id = id;
    }

    public string Id { get;  }
    public string? Username { get; set; }
    public string? Locale { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public bool IsDisabled { get; set; }

    public ICollection<AppRole> Roles { get; set; }

    public string? NormalizedUsername { get; set; }
    public string? Nickname { get; set; }
    public string? Title { get; set; }
    public string? ProfileUrl { get; set; }
    public string? Timezone { get; set; }
    public string? DisplayName { get; set; }
    public string? Formatted { get; set; }
    public string? HonorificSuffix { get; set; }
    public string? HonorificPrefix { get; set; }
    public string? MiddleName { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Organization { get; set; }
    public string? Division { get; set; }
    public string? CostCenter { get; set; }
    public string? UserType { get; set; }
    public ICollection<AppAddress>? Addresses { get; set; }
    public ICollection<AppPhoneNumber>? Phones { get; set; }
    public ICollection<AppEmail>? Emails { get; set; }
    public AppManager? Manager { get; set; }
}