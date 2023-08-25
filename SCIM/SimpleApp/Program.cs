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
    options.UseInMemoryDatabase("Application");
});

var scimServiceProviderBuilder =
    builder.Services
        .AddScimServiceProvider("/scim",
            new ScimLicensingOptions()
            {
                Licensee = "DEMO",
                LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjMtMDktMDRUMDA6MDA6MDAiLCJpYXQiOiIyMDIzLTA4LTA0VDE5OjIwOjQwIiwib3JnIjoiREVNTyIsImF1ZCI6OH0=.Or/7kSgVDGFeSYsdoQcfRl028AWIEyIhKI3QyhTBh772AuoHP/Mj1ufpu3hC1oylcwFZlyfgyIRpN1d5qQasHKMvzc8Nq8ILaGbS7SYqXNIFEhF0jaHcaFtG970mRa4DwU8lULac1kRcg6bvcrGRLD8VveBPwZFQ9ixxCLrEjNVSKTMoo4ilYqwEl1GkG0gcHFsnot7IPSruRBfo+oHzrO9Qa3Wv2mfoB5hyEpKz2UNCPW5gfA93krYcKfy4ExV0ZerqkB6NyPkgB6DOxNMftJjrqY/oFh9s35Q7H4ZpKnvvOkQCfKq5Xu6TCmF+EspzcXR7DI4bK6GYJa+5AyoCqAOU0mwprIoOaE9mEMImkxxgnXmZwtOdkLCCTxKHLDJ2WHX0Tqwbx+rMgBP8JRXJT5jx2V1nzjCtiNXFTIVAzGcKHrDCXFItEJk1nhQPuDTF1R4JYQnKX+t0ftfaZdmZYskC5ZM+lHB+tGAyCCykQYURzSTRl+l9Ycbb3gWoXbsa6Z6FLyp6rO0HzzAUpBIRePvvkvN5Ndjz/7VLcGdrWLuWLHeCp0JiR+WSd5bQMhUz0UMplh8dtvMAbPNLO2G7hkmWQ5w2Djh7jdEx5FtgTwUxlaQZS1LOplh8uahU3S8Bmr9lKko5uBu4dQsbmbiR577NTdk/80Fjdk5oIlSg7cc=" } , new ScimServiceProviderConfigOptions()
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