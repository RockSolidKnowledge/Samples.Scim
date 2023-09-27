using Microsoft.EntityFrameworkCore;
using Rsk.AspNetCore.Scim.Configuration;
using Rsk.AspNetCore.Scim.Configuration.DependencyInjection;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;
using SimpleApp.SCIM;
using SimpleApp.Services;

namespace SimpleApp;

public class Program
{
    public static void Main(params string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<AppDbContext>(options => { options.UseInMemoryDatabase("Application"); });

        var scimServiceProviderBuilder =
            builder.Services
                .AddScimServiceProvider("/scim",
                    new ScimLicensingOptions()
                    {
                        Licensee = "DEMO",
                        LicenseKey = "..."
                    }, new ScimServiceProviderConfigOptions()
                    {
                        FilteringSupported = false,
                        SortingSupported = true,
                        PatchSupported = true,
                        //  IgnoreMissingExtensionSchemas = true,
                        RolesAndEntitlements = new RolesAndEntitlementsOptions(
                            new AvailableRoleOptions(Enabled: true, PrimarySupported: true, TypeSupported: true, MultipleRolesSupported: true),
                            new AvailableEntitlementOptions(Enabled: true, PrimarySupported: true, TypeSupported: true, MultipleEntitlementsSupported: true)
                        )
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

        AddRoleAndEntitlementStores(scimServiceProviderBuilder);

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
    }

    private static void AddRoleAndEntitlementStores(IScimServiceProviderBuilder scimServiceProviderBuilder)
    {
        ScimAssignmentsStoreBuilder<EntitlementResource> entitlementStoreBuilder =
            new ScimAssignmentsStoreBuilder<EntitlementResource>();
        IScimAssignmentsStore<EntitlementResource> entitlementStore =
            entitlementStoreBuilder
                .AddAssignment("5", "All Printer Permissions",
                    c =>
                        c.AddAssignment("1", "Printing")
                            .AddAssignment("2", "Scanning")
                            .AddAssignment("3", "Copying")
                            .AddAssignment("4", "Collating")
                ).Build();

        ScimAssignmentsStoreBuilder<RoleResource> roleStoreBuilder = new ScimAssignmentsStoreBuilder<RoleResource>();

        IScimAssignmentsStore<RoleResource> roleStore =
            roleStoreBuilder.AddAssignment("global_lead", "Global Team Lead", gbl =>
                    gbl.AddAssignment("us_team_lead", "U.S. Team Lead", us =>
                        us.AddAssignment("nw_regional_lead", "Northwest Regional Lead")))
                .Build();

        scimServiceProviderBuilder.AddAssignmentStore(roleStore);
        scimServiceProviderBuilder.AddAssignmentStore(entitlementStore);
    }
}