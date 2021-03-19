using CustomStoreAndValidation.Stores;
using CustomStoreAndValidation.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Scim.Configuration;
using Rsk.AspNetCore.Scim.Models;

namespace CustomStoreAndValidation
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

            services.AddScimServiceProvider("/SCIM", licensingOptions)
                .AddResource<User, ScimStore, ScimValidator>("urn:ietf:params:scim:schemas:core:2.0:User", "Users")
                .AddScimDbContext(options => options.UseInMemoryDatabase(connectionString));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseScim();
        }
    }
}
