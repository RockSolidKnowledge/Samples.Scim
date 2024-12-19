using Microsoft.EntityFrameworkCore;
using Rsk.AspNetCore.Scim.Configuration;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Models;
using SimpleApp.SCIM;
using SimpleApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
});

var scimServiceProviderBuilder =
    builder.Services
        .AddScimServiceProvider("/scim",
            new ScimLicensingOptions()
            {
                Licensee = "DEMO",
                LicenseKey = "Get a license key form https://www.identityserver.com/products/scim-for-aspnet" } , new ScimServiceProviderConfigOptions()
            {
                FilteringSupported = false,
                SortingSupported = true,
                PatchSupported = true,
                EnableAzureAdCompatibility = true,
                PaginationOptions = new PaginationOptions(true, true, PaginationMethod.Index)
            })
        .AddResource<User, AppUserStore>(ScimSchemas.User, "users")
        .AddResourceExtension<User, EnterpriseUser>(ScimSchemas.EnterpriseUser)
     //   .AddResourceExtension<User,Employment>("urn:ietf:params:scim:schemas:extension:employment:2.0:User")
        .AddResource<Group, AppRoleStore>(ScimSchemas.Group, "groups")
        .AddFilterPropertyExpressionCompiler()
        .MapScimAttributes<AppUser>(ScimSchemas.User, mapper =>
        {
            mapper
                .Map("id", u => u.Id)
                .Map("userName", u => u.Username)
                .Map("active", u => u.IsDisabled, ScimFilterAttributeConverters.Inverse)
                .Map("displayName", u => u.DisplayName)
                .Map("title", u => u.Title)
                .Map("preferredLanguage", u => u.PreferredLanguage)
                .Map("userType", u => u.UserType)
                .Map("nickName", u => u.Nickname)
                .Map("userName", u => u.Username)
                .Map("timezone", u => u.Timezone)
                .Map("profileUrl", propertyAccessor: u => u.ProfileUrl)
                .Map("locale", u => u.Locale)
                .Map("name.givenName", u => u.FirstName)
                .Map("name.familyName", u => u.LastName)
                .Map("name.formatted", u => u.Formatted)
                .Map("name.middleName", propertyAccessor: u => u.MiddleName)
                .Map("name.honorificPrefix", propertyAccessor: u => u.HonorificPrefix)
                .Map("name.honorificSuffix", u => u.HonorificSuffix)
                .MapCollection<AppAddress>("addresses", u => u.Addresses, complexBuilder =>
                {
                    complexBuilder.Map("country", a => a.Country)
                        .Map("postalCode", a => a.PostalCode)
                        .Map("region", a => a.Region)
                        .Map("locality", a => a.Locality)
                        .Map("streetAddress", a => a.StreetAddress)
                        .Map("formatted", a => a.Formatted)
                        .Map("type", a => a.Type)
                        .Map("primary", a => a.Primary);
                })
                .MapCollection<AppPhoneNumber>("phoneNumbers", u => u.Phones, phoneBuilder =>
                {
                    phoneBuilder.Map("value", p => p.Value)
                        .Map("type", p => p.Type)
                        .Map("primary", p => p.Primary);
                })
                .MapCollection<AppEmail>("emails", u => u.Emails, emailBuilder =>
                {
                    emailBuilder.Map("value", e => e.Value)
                        .Map("type", e => e.Type)
                        .Map("primary", e => e.Primary);

                });
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
        })
        .MapScimAttributes<AppUser>(ScimSchemas.EnterpriseUser, mapBuilder =>
        {
            mapBuilder
                .Map($"department", u => u.Department)
                .Map($"employeeNumber", u => u.EmployeeNumber)
                .Map($"organization", u => u.Organization)
                .Map($"division", u => u.Division)
                .Map($"costCenter", u => u.CostCenter)
                .MapComplex("manager", u => u.Manager, managerBuilder =>
                {
                    managerBuilder.Map("value", m => m.Value)
                        .Map("displayName", m => m.Display);
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

using var scope = app.Services.CreateScope();
AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
DatabaseInitializer.Initialize(context);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();