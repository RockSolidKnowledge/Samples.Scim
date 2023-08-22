using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;
using Microsoft.Extensions.Options;
using Rsk.AspNetCore.Scim.Configuration;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Filters;
using Rsk.AspNetCore.Scim.Models;
using SimpleApp.SCIM;
using SimpleApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("Application");
});

ScimServiceProviderConfigOptions ServiceProviderConfigOptions = new()
{
    FilteringSupported = true,
    PatchSupported = true,
    SortingSupported = true,
    FilterMaxResourceCount = 100
};

var scimServiceProviderBuilder = 
builder.Services
    .AddScimServiceProvider("/scim",
        new ScimLicensingOptions()
        {
            Licensee = "DEMO",
            LicenseKey =
                "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjMtMDgtMDVUMDA6MDA6MDAiLCJpYXQiOiIyMDIzLTA3LTA1VDEzOjM1OjM1Iiwib3JnIjoiREVNTyIsImF1ZCI6OH0=.dWm6Ib4qe4HUx8vn8iK0iUvsmUTj+/7Xxo4dimzTXKKQrefi9UV9pAitN4X75IgdvUabtCb5b6iRfyfkvSwz3VZ3YplCZ/jnNuGSj3rzUJLIfECdCXcn3m7k86DGLzrgjVxB6QuxiRPk6B+LburNqhD+GYIeb52TKY+jymHvDLjubp80A915l91dYw/+6bNJC5JcDocdjfkRyHnvE2L/aRAiueSJR5zRBch8LVFBDylsfIEQt/qXw3QzLGQZbpcSPpxoZ7OPUcfwxv2cJHpoNK7+Jqd9l+ZEP8B/913F6AG4a5hJ3JBub+gsF07/cHjP2bLyXEKEFm9SVKhDmS9Bwjlhca4vpGtsyV1oqq2HcFKRjhSHnFIoQ+5bMrYWQr5/hfDzjLr9DIAccS+5LXgUyyEczdHwJYQWzxWo3jB7+7cUn24/MTyNXlVd2y2//vFSALJJNmDWMXfCYUUOCAK+P2jw2bbQF10th1A5CewDl7ovppCNBWiTD2Cbj3uzeIJqYCwSBE9WqqFKB+WpwUD81QBmmFARIqQYButohRsGmmMieK0zcdHec9Fk7iQaCv7KlSHqwPIVBTyEKPz8tKX8GSjssX2+Yo6Seh7AkKNUIOrHGLjxA3JsVPlVfzyspTQZcz/WtA5ZnhKL+LN7/jDZdHz2mp+HO+kmRjPBaUUL3Ag="
        }, ServiceProviderConfigOptions)
    .AddResource<User, AppUserStore>(ScimSchemas.User, "users")
    .AddResourceExtension<User, EnterpriseUser>(ScimSchemas.EnterpriseUser)
    .AddResource<Group, AppRoleStore>(ScimSchemas.Group, "groups")
    .AddFilterPropertyExpressionCompiler()
    .MapScimAttributes(ScimSchemas.User, mapper =>
    {
        mapper
            .Map<AppUser>("id", u => u.Id)
            .Map<AppUser>("userName", u => u.NormalizedUsername , ScimFilterAttributeConverters.ToUpper)
            .Map<AppUser>("name.familyName", u => u.LastName)
            .Map<AppUser>("name.givenName", u => u.FirstName)
            .Map<AppUser>("active", u => u.IsDisabled , ScimFilterAttributeConverters.Inverse)
            .Map<AppUser>("locale", u => u.Locale);
    })
    .MapScimAttributes(ScimSchemas.EnterpriseUser, mapper =>
    {
        mapper
            .Map<AppUser>("department", u => u.Department);
    })
    .MapScimAttributes(ScimSchemas.Group, mapper =>
    {
        mapper
            .Map<AppRole>("id", r => r.Id)
            .Map<AppRole>("displayName", r => r.Name)
            .Map<AppRole>("members", r => r.Members);

        mapper.Map<AppUserRole>("value", r => r.AppUserId);
    });


var app = builder.Build();

app.UseScim();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();