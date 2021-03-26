using InteractiveClient.Models;
using InteractiveClient.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Scim.Configuration;

namespace InteractiveClient
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton<IClientUserStore, InMemoryClientUserStore>();
            services.AddScoped<IUserService, UserService>();

            services.AddScimClient(new ScimClientConfiguration
                {
                    Licensee = "Demo",
                    LicenseKey = "..."
                })
                .AddUser<ClientUser, ClientUserMapper>()
                .AddServiceProvider("ServiceProviderName", "https://localhost:5000/SCIM/");
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}