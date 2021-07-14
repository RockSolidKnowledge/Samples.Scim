using System.Security.Claims;
using System.Threading.Tasks;
using idunno.Authentication.Basic;
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

            AddBasicAuth(services);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SalesOnly", policy =>
                {
                    policy.RequireClaim("department", "sales");
                });
            });

            services.AddScimServiceProvider("/SCIM", licensingOptions)
                .AddScimDefaultResourcesForInMemoryStore()
                .UseAuthentication(BasicAuthenticationDefaults.AuthenticationScheme);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseScim();
        }

        private static void AddBasicAuth(IServiceCollection services)
        {
            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                .AddBasic(options =>
                {
                    options.Events = new BasicAuthenticationEvents
                    {
                        OnValidateCredentials = context =>
                        {
                            if (context.Username == "UserName" &&
                                context.Password == "Password!321")
                            {
                                context.Principal = new ClaimsPrincipal();
                                context.Success();
                            }
                            else
                            {
                                context.Fail("Invalid credentials");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
