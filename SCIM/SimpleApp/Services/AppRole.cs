namespace SimpleApp.Services;

public class AppRole
{
    public AppRole()
    {
        Id = Guid.NewGuid().ToString();
        Members = new List<AppUser>();
        Name = String.Empty;
    }

    public string Id { get;  }
    public string Name { get; set; }
    public ICollection<AppUser> Members { get; set; }
}