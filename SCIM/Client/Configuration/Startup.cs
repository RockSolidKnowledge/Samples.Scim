using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsk.AspNetCore.Scim.Configuration;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;
using Shared.Mappers;
using Shared.Models;
using Shared.Services;
using Shared.Stores;

namespace Configuration
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
            services.AddScoped<IResourceMapper<ClientUser, User>, ClientUserMapper>();

            services.AddLogging();

            services.AddSingleton<IStore<ClientUser>, InMemoryStore<ClientUser>>();

            services.AddScimClient(new ScimClientConfiguration
                {
                    Licensee = "Demo",
                    LicenseKey = "..."
                })
                .AddUser<ClientUser, ClientUserMapper>()
                .AddServiceProvider("ServiceProviderName", "https://localhost:5000/SCIM/")
                .ConfigurePrimaryHttpMessageHandler("ServiceProviderName", () => new HttpClientHandler
                {
                    Proxy = new WebProxy("http://127.0.0.1:8888")
                    {
                        Credentials = new NetworkCredential("1", "1")
                    },
                    UseProxy = true
                });

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
