## Running the sample

You can run the sample through your IDE, or from the command line. 

```bash
dotnet run
```

## Sample Requests

The sample requests live in the Samples.Scim/Postman/RolesAndEntitlements folder. There are six requests

- Get Roles Configuration
  - Retrieves the roles that have been setup using the builder in Program.cs
- Get Entitlements Configuration
  - Retrieves the entitlements that have been setup using the builder in Program.cs
- Add User
  - Adds a user with roles and entitlements
- Get User
  - Retrieves the user added by the Add User request, including the roles and entitlements
- Add User Fails Role Validation
  - Attempts to add a user with invalid roles, as configured in Program.cs
  - This request will fail
- Get User Fails Entitlement Validation
  - Attempts to add a user with invalid entitlements, as configured in Program.cs
  - This request will fail