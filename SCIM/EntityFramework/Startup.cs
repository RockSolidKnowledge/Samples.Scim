using EntityFramework.Contexts;
using EntityFramework.Entities;
using EntityFramework.Mappers;
using EntityFramework.Resources;
using Extensions.ResourceExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Scim.Configuration;

namespace EntityFramework
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            var licensingOptions = new ScimLicensingOptions
            {
                Licensee = "Demo",
                LicenseKey = "..."
            };

            var connectionString = configuration.GetConnectionString("ExtendedScim");

            services.AddScim("/SCIM", licensingOptions)
                .AddScimDefaultResourcesForEntityFrameworkStore()
                .AddResourceForDefaultEfStore<Organization, OrganizationEntity, OrganizationMapper>(
                    "urn:ietf:params:scim:schemas:RSK:2.0:Organization", "Organizations")
                .AddResourceExtension<Organization, CharityOrganization>(
                    "urn:ietf:params:scim:schemas:RSK:extension:charity:2.0:Organization")
                .AddScimDbContext<ExtendedScimDbContext>(options => options.UseSqlServer(connectionString));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseScim();
        }
    }
}
