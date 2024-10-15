namespace SimpleApp.Services;

public static class DatabaseInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Users.Any())
        {
            return;
        }

        var roles = new[]
        {
            new AppRole { Name = "Admin" },
            new AppRole { Name = "User" }
        };

        context.Roles.AddRange(roles);
        context.SaveChanges();

        Enumerable.Range(1, 50)
            .Select(i => AddUser(context))
            .ToList();
    }

    private static async Task AddUser(AppDbContext context)
    {
        var user = new AppUser
        {
            Username = Faker.User.Username(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            Locale = "en-US",
            IsDisabled = false,
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }
}