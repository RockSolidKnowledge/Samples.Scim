using Extensions.ResourceExtensions;
using Extensions.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Scim.Configuration;

namespace Extensions
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var licensingOptions = new ScimLicensingOptions
            {
                Licensee = "Demo",
                LicenseKey = "..."
            };

            services.AddScimServiceProvider("/SCIM", licensingOptions)
                .AddScimDefaultResourcesForInMemoryStore()
                .AddResource<Organization>("urn:ietf:params:scim:schemas:RSK:2.0:Organization", "Organizations")
                .AddResourceExtension<Organization, CharityOrganization>("urn:ietf:params:scim:schemas:RSK:extension:charity:2.0:Organization");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseScim();
        }
    }
}
