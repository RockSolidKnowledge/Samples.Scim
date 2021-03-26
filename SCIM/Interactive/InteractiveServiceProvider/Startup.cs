using System.Collections.Generic;
using InteractiveServiceProvider.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Scim.Configuration;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;

namespace InteractiveServiceProvider
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var licensingOptions = new ScimLicensingOptions
            {
                Licensee = "Demo",
                LicenseKey = "..."
            };

            services.AddScimServiceProvider("/SCIM", licensingOptions)
                .AddScimDefaultResourcesForInMemoryStore();

            // Our ScimStore doesn't support `GetAll` method. This custom implementation is only used for this purpose here.
            // Otherwise, you shouldn't need to add a custom in-memory implementation.
            services.AddSingleton<ICollection<User>>(new List<User>());
            services.AddSingleton<CustomScimStore<User>>();
            services.AddSingleton<IScimStore<User>>(s => s.GetService<CustomScimStore<User>>());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseScim();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}