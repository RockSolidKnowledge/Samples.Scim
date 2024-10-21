using Microsoft.EntityFrameworkCore;
using SimpleApp.Models;

#nullable disable
namespace SimpleApp.Services;

public class AppUserRole
{
    public string AppUserId { get; set; }
    public string AppRoleId { get; set; }

    public AppUser User { get; set; }
    public AppRole Role { get; set; }
}

public class AppDbContext : DbContext
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<AppRole> Roles { get; set; }
    public DbSet<AppPhoneNumber> Phones { get; set; }
    public DbSet<AppAddress> Addresses { get; set; }
    public DbSet<AppUserRole> UserRoles { get; set; }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .Property(au => au.Username)
            .UseCollation("NOCASE");

        modelBuilder.Entity<AppUser>()
            .Property(au => au.FirstName)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.LastName)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.Locale)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.Nickname)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.Title)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.ProfileUrl)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.Timezone)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.DisplayName)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.HonorificSuffix)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.HonorificPrefix)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.MiddleName)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .Property(au => au.PreferredLanguage)
            .IsRequired(false);

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Address);

        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.Phones);

        modelBuilder.Entity<AppAddress>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<AppPhoneNumber>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<AppUser>()
                .Property(au => au.Id);

        modelBuilder.Entity<AppRole>()
            .Property(ar => ar.Id);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasMany<AppRole>(u => u.Roles)
            .WithMany(r => r.Members)
            .UsingEntity<AppUserRole>(x => x.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.AppRoleId),
                x => x.HasOne(x => x.User).WithMany().HasForeignKey(x => x.AppUserId));

        modelBuilder.Entity<AppRole>()
            .HasMany(r => r.Members)
            .WithMany(u => u.Roles)
            .UsingEntity<AppUserRole>(x=>x.HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.AppUserId),
                x=>x.HasOne(x=>x.Role).WithMany().HasForeignKey(x=>x.AppRoleId));

        modelBuilder.Entity<AppUserRole>()
            .HasKey(x => new { x.AppUserId, x.AppRoleId });
    }
}