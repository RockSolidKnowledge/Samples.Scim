using In_Memory.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Scim.Configuration;

namespace In_Memory
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var licensingOptions = new ScimLicensingOptions
            {
                Licensee = "DEMO",
                LicenseKey =
                    "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjMtMDgtMDVUMDA6MDA6MDAiLCJpYXQiOiIyMDIzLTA3LTA1VDEzOjM1OjM1Iiwib3JnIjoiREVNTyIsImF1ZCI6OH0=.dWm6Ib4qe4HUx8vn8iK0iUvsmUTj+/7Xxo4dimzTXKKQrefi9UV9pAitN4X75IgdvUabtCb5b6iRfyfkvSwz3VZ3YplCZ/jnNuGSj3rzUJLIfECdCXcn3m7k86DGLzrgjVxB6QuxiRPk6B+LburNqhD+GYIeb52TKY+jymHvDLjubp80A915l91dYw/+6bNJC5JcDocdjfkRyHnvE2L/aRAiueSJR5zRBch8LVFBDylsfIEQt/qXw3QzLGQZbpcSPpxoZ7OPUcfwxv2cJHpoNK7+Jqd9l+ZEP8B/913F6AG4a5hJ3JBub+gsF07/cHjP2bLyXEKEFm9SVKhDmS9Bwjlhca4vpGtsyV1oqq2HcFKRjhSHnFIoQ+5bMrYWQr5/hfDzjLr9DIAccS+5LXgUyyEczdHwJYQWzxWo3jB7+7cUn24/MTyNXlVd2y2//vFSALJJNmDWMXfCYUUOCAK+P2jw2bbQF10th1A5CewDl7ovppCNBWiTD2Cbj3uzeIJqYCwSBE9WqqFKB+WpwUD81QBmmFARIqQYButohRsGmmMieK0zcdHec9Fk7iQaCv7KlSHqwPIVBTyEKPz8tKX8GSjssX2+Yo6Seh7AkKNUIOrHGLjxA3JsVPlVfzyspTQZcz/WtA5ZnhKL+LN7/jDZdHz2mp+HO+kmRjPBaUUL3Ag="
            };
            
            ScimServiceProviderConfigOptions ServiceProviderConfigOptions = new()
            {
                FilteringSupported = true,
                PatchSupported = true,
                SortingSupported = true,
                FilterMaxResourceCount = 100
                // IgnoreMissingExtensionSchemas = true
            };

            services.AddScimServiceProvider("/SCIM", licensingOptions, ServiceProviderConfigOptions)
                .AddScimDefaultResourcesForInMemoryStore()
                .AddResource<Organization>("urn:ietf:params:scim:schemas:RSK:2.0:Organization", "Organizations");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseScim();
        }
    }
}
