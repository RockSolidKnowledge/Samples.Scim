using In_Memory.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Scim.Configuration;

namespace In_Memory
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var licensingOptions = new ScimLicensingOptions
            {
                Licensee = "DEMO",
                LicenseKey = "..."
            };
            
            ScimServiceProviderConfigOptions serviceProviderConfigOptions = new()
            {
                FilteringSupported = true,
                PatchSupported = true,
                SortingSupported = true,
                FilterMaxResourceCount = 100
                // IgnoreMissingExtensionSchemas = true
            };

            services.AddScimServiceProvider("/SCIM", licensingOptions, serviceProviderConfigOptions)
                .AddScimDefaultResourcesForInMemoryStore()
                .AddResource<Organization>("urn:ietf:params:scim:schemas:RSK:2.0:Organization", "Organizations");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseScim();
        }
    }
}
