using System;
using Caching.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsk.AspNetCore.Scim.Authenticators;
using Rsk.AspNetCore.Scim.Configuration.DependencyInjection;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;
using Shared.Mappers;
using Shared.Models;
using Shared.Services;
using Shared.Stores;

namespace Caching
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IResourceMapper<ClientUser, User>, ClientUserMapper>();

            services.AddLogging();

            services.AddSingleton<IStore, InMemoryStore>();

            services.AddScimClient()
                .AddUser<ClientUser, ClientUserMapper>()
                .AddCache<CustomCache>();

            services.AddControllers();
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
