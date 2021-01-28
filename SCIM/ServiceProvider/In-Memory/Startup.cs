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
                Licensee = "Demo",
                LicenseKey = "..."
            };

            services.AddScim("/SCIM", licensingOptions)
                .AddScimDefaultResourcesForInMemoryStore()
                .AddResource<Organization>("urn:ietf:params:scim:schemas:RSK:2.0:Organization", "Organizations");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseScim();
        }
    }
}
