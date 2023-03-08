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

var scimServiceProviderBuilder = 
builder.Services
    .AddScimServiceProvider("/scim",
        new ScimLicensingOptions()
        {
            Licensee = "DEMO",
            LicenseKey =
                "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjMtMDMtMjdUMDA6MDA6MDAiLCJpYXQiOiIyMDIzLTAyLTI3VDExOjAyOjIxIiwib3JnIjoiREVNTyIsImF1ZCI6OH0=.WVB+bj/KUUBuGgoIOK7EwnT97T2su4ovkXaOvxUTL2FKHa1Jfphb2MP0RB2ozq/0WedoyotSEAE2H0jJ00GN+FcNbn0/LmnCwjKcWGV+8970uFWaVCKE/zcj96894fsk/x4br3mF/6b92D+7K6vcKqMOPs/Oxd0uIH6/PESdQZNGXlMt/ac+rlPigxI63QtDClLtSMr1Cojd9w+zrvyIuAQdL+IQhKATxQZ9MEsEt2uogAHV1m3HoVl715LAdL0gjBqQwiVx0GvewDrIzFM1X+R90irg2Uo6Kq9y57vBbdGZlJuq/XQgHoAGptduE85jHequUgUFLO0J4WdbTUaf+B7TTSeAKe9JLAGTE9cTb4zqGGGGR87+B5+Fh7hqcvpjusjySaxRBTATgA3FBoaVvE2UmF7q3q9ThAjbOPHojTHzhWIM4w5+CCI81y5vUJ0S9kEw3L8iqEqzrYqkitRKMBV5nWPmpBIw/wufM/47ihNjobKz0XWddudeYnZBA/B27IhVD/J2RMpTleRDZTaKenm2OOxtrFRUmo4My+cEIRUEK8jjPlwBfwtN74OBSXlY2FV2/ME8QUwARL4Aa03uLoc8WFFHNcP1SXg0Yxyb3ShUdgQvgaVaJaQQU9QUggCU/2hT+LYHtAhrmeSX6LkvQUhgijGZJzC2qQeuNth1HCM="
        })
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