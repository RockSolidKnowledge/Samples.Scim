using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsk.AspNetCore.Scim.Authenticators;
using Rsk.AspNetCore.Scim.Configuration.DependencyInjection;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Interfaces;
using Rsk.AspNetCore.Scim.Models;
using Shared.Mappers;
using Shared.Models;
using Shared.Services;
using Shared.Stores;

namespace Authentication
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

            var builder = services.AddScimClient()
                .AddUser<ClientUser, ClientUserMapper>();

            ConfigureForApiKey(builder);
            //ConfigureForOAuth(builder);
            //ConfigureForBasicAuth(builder);

            //ConfigureForCustomAuth(builder);

            services.AddControllers();
        }

        private void ConfigureForBasicAuth(IScimClientBuilder builder)
        {
            ScimBasicAuthOptions basicAuthOptions = new ScimBasicAuthOptions("UserName", "Password!321");

            builder.AddServiceProvider("ServiceProviderName", "https://localhost:5000/SCIM/", basicAuthOptions);
        }

        private void ConfigureForOAuth(IScimClientBuilder builder)
        { 
            ScimOAuthOptions oAuthOptions = new ScimOAuthOptions("scimclient", "https://ids.local:5003/connect/token", "scimclient", "scim");

            builder.AddServiceProvider("ServiceProviderName", "https://localhost:5000/SCIM/", oAuthOptions);
        }

        private void ConfigureForApiKey(IScimClientBuilder builder)
        {
            ScimApiKeyOptions apiKeyOptions = new ScimApiKeyOptions("x-scim-api-key", "Password!321");

            builder.AddServiceProvider("ServiceProviderName", "https://localhost:5000/SCIM/", apiKeyOptions);
        }

        private void ConfigureForCustomAuth(IScimClientBuilder builder)
        {
            builder.AddScimServiceProvider<Authenticator.Authenticator>("ServiceProviderName", options =>
            {
                options.BaseAddress = new Uri("https://localhost:5000/SCIM/");
                options.DefaultRequestHeaders.Add("Accept", ScimConstants.ContentType);
            });
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
