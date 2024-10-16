# Sample projects implementing RSK SCIM component

The Rock Solid Knowledge SCIM component for ASP.NET Core enables your web applications to act as a SCIM Client and Service Provider.

The SCIM component is available from [https://www.identityserver.com/products/scim-for-aspnet](https://www.identityserver.com/products/scim-for-aspnet).

## Projects

The sample projects implement the QuickStarts from [https://www.identityserver.com/documentation/scim/](https://www.identityserver.com/documentation/scim/). For more information about each project, please refer to the QuickStart documentation. You can enable bulk support in any of these projects by passing config options to the `AddScimServiceProvider` call.

Please note that the Service Provider implementations build upon one another based on the QuickStarts.

- **ServiceProvider**
	- **In-Memory:** A basic implementation of a SCIM Service Provider using the in-memory store, note no patch or filter support, and not compatible with AzureAD and Okta.
	- **Extensions:** A SCIM Service Provider implementation utilizing the extensions feature of SCIM
	- **AuthenticationAndAuthorization:** A SCIM Service Provider implementation with the SCIM endpoints protected with authentication and authorization
- **SimpleApp** A SCIM Service Provider implementation with custom stores that fully support filters and patch. This is compatible with AzureAD and Okta
	- This app is also implements and configures both cursor-based and index-based pagination, as per the [draft specification](https://datatracker.ietf.org/doc/draft-ietf-scim-cursor-pagination)
- **Rsk.ScimExamples.postman_collection.json** contains SCIM example requests

## Getting Started

- [Gettings Started article](https://www.identityserver.com/articles/managing-identities-across-cloud-based-applications-and-services-with-scim)
- [Documentation](https://www.identityserver.com/documentation/scim/)

## License Keys

For a demo license, please sign up on our [products page](https://www.identityserver.com/products/scim-for-aspnet), or reach out to <sales@identityserver.com>.
