using Microsoft.EntityFrameworkCore;
using Rsk.AspNetCore.Scim.Configuration;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Hosting.Tenancy;
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

var scimServiceProviderBuilder =
    builder.Services
        .AddScimServiceProvider("/scim",
            new ScimLicensingOptions()
            {
                Licensee = "DEMO",
                LicenseKey = "Get a license key form https://www.identityserver.com/products/scim-for-aspnet"
            }, new ScimServiceProviderConfigOptions()
            {
                FilteringSupported = false,
                SortingSupported = true,
                PatchSupported = true,
                //  IgnoreMissingExtensionSchemas = true
            })
        .AddResource<User, AppUserStore>(ScimSchemas.User, "users")
        .AddResourceExtension<User, EnterpriseUser>(ScimSchemas.EnterpriseUser)
        //   .AddResourceExtension<User,Employment>("urn:ietf:params:scim:schemas:extension:employment:2.0:User")
        .AddResource<Group, AppRoleStore>(ScimSchemas.Group, "groups")
        .AddFilterPropertyExpressionCompiler()
        .AddPathBasedTenancy(PathBasedTenancyRequirement.CanBePresent)
        .MapScimAttributes<AppUser>(ScimSchemas.User, mapper =>
        {
            mapper
                .Map("id", u => u.Id)
                .Map("userName", u => u.Username)
                .Map("name.familyName", u => u.LastName)
                .Map("name.givenName", u => u.FirstName)
                .Map("active", u => u.IsDisabled, ScimFilterAttributeConverters.Inverse)
                .Map("locale", u => u.Locale);
        })
        .MapScimAttributes<AppUser>(ScimSchemas.EnterpriseUser, mapper =>
        {
            mapper
                .Map("department", u => u.Department);
        })
        .MapScimAttributes<AppRole>(ScimSchemas.Group, mapper =>
        {
            mapper
                .Map("id", r => r.Id)
                .Map("displayName", r => r.Name)
                .MapCollection<Member>("members", r => r.Members, member =>
                {
                    member
                        .Map("value", m => m.Value)
                        .Map("display", m => m.Display)
                        .Map("type", m => m.Type);
                });

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