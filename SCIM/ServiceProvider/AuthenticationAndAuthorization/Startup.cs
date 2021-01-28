using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Scim.Configuration;

namespace AuthenticationAndAuthorization
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

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SalesOnly", policy =>
                {
                    policy.RequireClaim("department", "sales");
                });
            });

            services.AddScim("/SCIM", licensingOptions)
                .AddScimDefaultResourcesForInMemoryStore()
                .UseAuthentication(CookieAuthenticationDefaults.AuthenticationScheme, "SalesOnly");
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseScim();
        }
    }
}
