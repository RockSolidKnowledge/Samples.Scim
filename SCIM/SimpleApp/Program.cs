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
                LicenseKey = "eyJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjQtMTEtMDdUMDE6MDA6MDIuNjgwMTM3KzAwOjAwIiwiaWF0IjoiMjAyNC0xMC0wOFQwMTowMDowMiIsIm9yZyI6IkRFTU8iLCJhdWQiOjh9.kI9JxoIPs1k5gbBj03grrDpdS9q9jssUKAAFsPthHODPYl6Hwf0OLX8Y+W4Iy3gXf0cvuSsyB7eH38VHjuHkt1X1U2MWFp67IP0IgrPWlJTSDxU0f9lzzWFKucV1kKeMNnUM9IPbHfOHaU3kAbpJcX24rkNbJXz0J9vNJlRFE/luLq4b1okob9qszN6hKKpV7nbThlRYzLnSKObwEag4P2fBMAzGrkWwsNjJpF4u9+1wYfqacIThPmta33ebfXMZUj7EOexD9wuSTgXkEA/qfv5Ix0E7ukDvoqJUQzAPqvMw6DARj6Kpy/gVbIloMN0kMmeGsdVE+mqgDTzTqxJhIElUt+RCNZ0W6wQIfNbJyksYK3gjHIMzUHGV4RYtWa2q9NOB5DF+Pp7k+FkkznroeIqWmaF8Dbp6AAbMDgGfXyjM2pJzdL7pHyb1FFOZl4t2pCZmF1xOmDyGvoRSiCf0I/VyD+HfK9oGEjitZS9blBxX1K82gDBHrnuhaDntP0C+g65Vb5Wv8ZdgjFJne/CzdsXiBWvswA/UKIJGqfM/76mn5r3vcgV2tYsu3a9iGtveTOJ9v53maX6NWJYyiiWRtwaG0usTpUvlt80K8ccHf4gW3ktVc9OLpfZPyD8/nA+T27Jepu80v3PjMx1Sy+tgOWFV6Xqdh1QSY8RCGfVUgrI=" } , new ScimServiceProviderConfigOptions()
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
                .Map("name.familyName", u => u.LastName)
                .Map("name.givenName", u => u.FirstName)
                .Map("active", u => u.IsDisabled, ScimFilterAttributeConverters.Inverse)
                .Map("locale", u => u.Locale);
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
        .MapPatchingOperations<AppUser>(ScimSchemas.User, mapBuilder =>
        {
            mapBuilder.Map("userName", u => u.Username);
            mapBuilder.Map("active", u => u.IsDisabled, ScimFilterAttributeConverters.Inverse);
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