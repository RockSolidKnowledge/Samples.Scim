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
                "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjItMDUtMDhUMDA6MDA6MDAiLCJpYXQiOiIyMDIyLTA0LTA4VDEwOjMwOjE5Iiwib3JnIjoiREVNTyIsImF1ZCI6OH0=.fpNhR/DUV2KwQtbL/AKZTlvp8Itfv0UVCbqFDJP7TWpaf+cMS7MElLUuvUDB1NH8zu0TdgH0Lk2fW+BcmYsS0SdN6HwJ8kVr8sqefX0xNWiRqT/zNnxZUpDzP44ZVwZ1yuf5OaToKKoiGL3PksB72G5kp6rJdHYTBNlF43agkk791Iddlo9A9g8402OUYsq8qpo4qyBHkhmmAuHRNHk1221f1g1uDLhzefLq+ybCkc/JTpnKxnDINw3nmXraKTLWdPVXGvncvyhVwpw7+OFtGu8RIEItL9IvolZX6ATdimSv003tncMJcQmRZQ0hN7MkifaMwuksC2PYz9J4aJHGyBEgiz1woim+OqNs/MNiK5lDu99KOqqkm/2Y9XB649nwPmv/TjLysJRYp0SF9nzTwq70ncARBv0DsCatzOOObXFwzb7dSiIsMByF01hZFd5jvBoSI/WJsT393AP17l/PX1Cs0b7XEeb4haLEOfSLxByG/VouboELyseY7NY17MYzDLEGJ58yaWC9pvbYPI1afeAx/rnEFOT2rtH1Z7FoB9AJWLtEowRdcdvFu7d/9WnwhnMcVhQ899kPoDgcnnfCbD+IKBMItmsdu9LcIv4IflWxB0EIHKO3pJOGaYxrNa1koaPyjSikQUwJx+IrrCAYujas3Taj5clDVtd9EH6QnM4="
        })
    .AddResource<User, AppUserStore>(ScimConstants.Schemas.User, "users")
    .AddResourceExtension<User, EnterpriseUser>(ScimConstants.Schemas.EnterpriseUser)
    .AddResource<Group, AppRoleStore>(ScimConstants.Schemas.Group, "groups")
    .AddFilterPropertyExpressionCompiler()
    .MapScimAttributes(ScimConstants.Schemas.User, mapper =>
    {
        mapper
            .Map<AppUser>("id", u => u.Id)
            .Map<AppUser>("userName", u => u.Username)
            .Map<AppUser>("name.familyName", u => u.LastName)
            .Map<AppUser>("name.givenName", u => u.FirstName)
            .Map<AppUser>("active", u => u.IsActive)
            .Map<AppUser>("locale", u => u.Locale);
    })
    .MapScimAttributes(ScimConstants.Schemas.EnterpriseUser, mapper =>
    {
        mapper
            .Map<AppUser>("department", u => u.Department);
    })
    .MapScimAttributes(ScimConstants.Schemas.Group, mapper =>
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