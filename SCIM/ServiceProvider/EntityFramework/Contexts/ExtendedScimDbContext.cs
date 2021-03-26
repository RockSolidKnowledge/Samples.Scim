using EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Rsk.AspNetCore.Scim.EntityFramework;

namespace EntityFramework.Contexts
{
    public class ExtendedScimDbContext : ScimDbContext
    {
        public virtual DbSet<OrganizationEntity> Organizations { get; set; }

        public ExtendedScimDbContext(DbContextOptions<ExtendedScimDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrganizationEntity>(builder => builder.HasKey(o => o.Id));
        }
    }
}
