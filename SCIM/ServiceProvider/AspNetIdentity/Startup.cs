using AspNetIdentity.Contexts;
using AspNetIdentity.Mappers;
using AspNetIdentity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Scim.Configuration;

namespace AspNetIdentity
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

            var connectionString = configuration.GetConnectionString("Identity");

            services.AddDbContext<MyIdentityContext>(options =>
                options.UseInMemoryDatabase(connectionString));

            services.AddIdentityCore<MyIdentityUser>()
                .AddUserStore<UserStore<MyIdentityUser, IdentityRole, MyIdentityContext>>()
                .AddEntityFrameworkStores<MyIdentityContext>();

            services.AddScimServiceProvider("/SCIM", licensingOptions)
                .AddUserResourceWithDefaultIdentityEfStore<MyIdentityUser, MyIdentityMapper>("Users");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseScim();
        }
    }
}
