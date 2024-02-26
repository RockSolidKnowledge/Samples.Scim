using Rsk.AspNetCore.Scim.Configuration;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

IScimAssignmentsStore<RoleResource> roleStore = roleStoreBuilder.AddAssignment("global_lead", "Global Team Lead", gbl =>
        gbl.AddAssignment("us_team_lead", "U.S. Team Lead", us =>
            us.AddAssignment("nw_regional_lead", "Northwest Regional Lead")))
    .Build();

builder.Services
    .AddScimServiceProvider(
        "/scim",
        new ScimLicensingOptions("Demo",
            "Get a license key form https://www.identityserver.com/products/scim-for-aspnet"),
        new ScimServiceProviderConfigOptions
        {
            RolesAndEntitlements = new RolesAndEntitlementsOptions(
                
                new AvailableRoleOptions(Enabled: true, PrimarySupported: true, TypeSupported: false,
                    MultipleRolesSupported: false),
                
                new AvailableEntitlementOptions(Enabled: true, PrimarySupported: false, TypeSupported: false,
                    MultipleEntitlementsSupported: true)
            )
        })
    .AddScimDefaultResourcesForInMemoryStore()
    .AddAssignmentStore(roleStore)
    .AddAssignmentStore(entitlementStore);

WebApplication app = builder.Build();

app.UseScim();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Run();