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
                    LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjEtMDQtMDFUMDA6MDA6MDYuMzM1NDkrMDE6MDAiLCJpYXQiOiIyMDIxLTAzLTAyVDAwOjAwOjA2Iiwib3JnIjoiREVNTyIsImF1ZCI6OH0=.EGV/YuJVu3ezqKcAWENGFMM0gfBjSPdfrwJTQDLDa2Kyu5x3yD6yAerkh8Sp0B6Wi1PjQYNpO1AWGTvecJFtE76jPI0f3D2kgbtz7hSSxD+YpanwDLi0gkGn7t3hM0TJGtXU9X+y7ssqFFZLqlp1RhpBiNIzY/culjQDcjkNIjtjdAV/VfDtJTNovttig+SAwvA4B8VkkoMKLaEpQ241xUN4D1e+elwPyyV0x4LuVxtB9Z6JWgPAK7TlWuob7O3wWQss4kHYuDI1FPe5LUyPDP1AMyUsMBbgpOibDNHj8QG6ne9YKOlAE96saKHZ4Z7+KcIaLtSjHpI6jZaQlkN7yNSu5Sna6o6BcsGeVZsAb/0r2vBGVenB9v1APFgBXQTTphvqD7l5aMgAez0xNcG6TOG/lmLwwFfKNyCD0J1aDrErs1e/GdnEOFRfK77UqjKFkoYIDerwDuqtlkjqdRC0bW2Xq068sbkDxypDajrP4Pp6K19bQ9FBFO3Tg/sKOn5LnV5zwAT9rRC2eP1u7kYZLwRZLUt18XMKdEfyWEEWS0D65yuxgFhDGJ+ew69VWAnkzxyTGC9YOortXyutSuxenPHGrvmYnaT2HOyxW17NQIphmF42gndO7I0hJwoY6j4c6aOyzGrec0erPyehSt+iH1GwKc9J7jHk3Q2PQFh8XLQ="
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