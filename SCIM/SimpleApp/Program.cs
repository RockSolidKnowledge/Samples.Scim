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

var scimServiceProviderBuilder = 
builder.Services
    .AddScimServiceProvider("/scim",
        new ScimLicensingOptions()
        {
            Licensee = "DEMO",
            LicenseKey =
                "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjItMDYtMDhUMDA6MDA6MDAiLCJpYXQiOiIyMDIyLTA1LTA4VDE0OjMzOjExIiwib3JnIjoiREVNTyIsImF1ZCI6OH0=.HanfXDuwStGbNanJGe/40iFwUzjnJ/bLjcIPcwlDbmuwOTT6f3H9RXf1U7CkpplefsLlueYQ5AJHz1tzrJzIkVbCOtBAC3zl8WIDU4uNXC4SeyyVD605NJDNZMLokbyGAkh1T8hwaX1J54Pcm2essUELRUWzVSNQZFZpDdfD+qU+9EsfyVa+xk15gy+ERC49mdAq1LTKETux0gjWQ93++6oxY6hTJ0OL1hkZ39Fh4UAWPRZIhfoACYZ2i2qVIwwJd2cq4mbyEfVTbF4z4eL8pycw6MtERd9GwEoafjUYDp5Z12CnqrF/XFuVEWXtfUhkL1wA7/bEBdheNHWWI21x2SwmjVGHYlc2RXXT1dX0cxBEKzfbID4l9Ee2aZH5QyoZ5UPuk2UWXvHSf1CP4djRVSDKxHPnfPhhqFteo+nKislTi5dQ0x6yZwxlsP0PsrFrzDJy2jEOpShRQGkKB+e/lhTGfV5pHxItYFf9JZ2aTDgQHJoFOLbU4m2JztlsRzyO9UCCJvuqtWXLPsX6Q6HHfYlcJ48ZK3A14hw79Bo2KrIefLol39Xp/VyD4a4BK4kkCWDaWsG0/fCTO+Y1cxE9p5EsVRpL/52QTj3+Qtird0spAqPrLdim7K9ftdYu6MHkrWCj+JQI/tgtKH592PrGfrVoyAztf2BWbp2LEQzGGj0="
            
        })
    .AddResource<User, AppUserStore>(ScimSchemas.User, "users")
    .AddResourceExtension<User, EnterpriseUser>(ScimSchemas.EnterpriseUser)
    .AddResource<Group, AppRoleStore>(ScimSchemas.Group, "groups")
    .AddFilterPropertyExpressionCompiler()
    .MapScimAttributes(ScimSchemas.User, mapper =>
    {
        mapper
            .Map<AppUser>("id", u => u.Id)
            .Map<AppUser>("userName", u => u.Username)
            .Map<AppUser>("name.familyName", u => u.LastName)
            .Map<AppUser>("name.givenName", u => u.FirstName)
            .Map<AppUser>("active", u => u.IsActive)
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