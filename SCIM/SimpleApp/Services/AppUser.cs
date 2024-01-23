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

    public string Tenancy { get; set; } = String.Empty;
    
    public string Id { get;  }
    public string? Username { get; set; }
    public string? Locale { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public bool IsDisabled { get; set; }
    
    public ICollection<AppRole> Roles { get; set; }

    public string? NormalizedUsername { get; set; }
}