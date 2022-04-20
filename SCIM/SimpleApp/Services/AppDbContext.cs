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
            .Property(au => au.FirstName)
            .IsRequired(false);
    
        modelBuilder.Entity<AppUser>()
            .Property(au => au.LastName)
            .IsRequired(false);
    
        modelBuilder.Entity<AppUser>()
            .Property(au => au.Locale)
            .IsRequired(false);

        var id = modelBuilder.Entity<AppUser>()
                .Property(au => au.Id);

        modelBuilder.Entity<AppRole>()
            .Property(ar => ar.Id);


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