using DefaultResources.Mappers;
using DefaultResources.Models;
using DefaultResources.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;
using Shared.Mappers;
using Shared.Models;
using Shared.Services;
using Shared.Stores;
using ClientGroup = DefaultResources.Models.ClientGroup;

namespace DefaultResources
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
            services.AddScoped<IClientService<ClientUser, User>, ClientService<ClientUser, User>>();
            
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IResourceMapper<ClientUser, User>, ClientUserMapper>();
            services.AddScoped<IResourceMapper<ClientGroupDto, Group>, ClientGroupMapper>();

            services.AddLogging();
            
            services.AddSingleton<IStore<ClientUser>, InMemoryStore<ClientUser>>();
            services.AddSingleton<IStore<ClientGroup>, InMemoryStore<ClientGroup>>();

            services.AddScimClient()
                .AddDefaultResources<ClientUser, ClientGroupDto, ClientUserMapper, ClientGroupMapper>()
                .AddServiceProvider("ServiceProviderName", "https://localhost:5000/SCIM/");

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
