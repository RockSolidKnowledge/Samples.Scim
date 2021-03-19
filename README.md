# Sample projects implementing RSK SCIM component

The Rock Solid Knowledge SCIM component for ASP.NET Core allows you to enable your web applications to act as a SCIM Client and Service Provider.

The SCIM component is available from [https://www.identityserver.com/products/scim-for-aspnet](https://www.identityserver.com/products/scim-for-aspnet).

## Projects

The sample projects implement the QuickStarts from [https://www.identityserver.com/documentation/scim/](https://www.identityserver.com/documentation/scim/). For more information about each project, please refer to the QuickStart documentation.

- **Client**
	- **DefaultResources:** A basic implementation of a SCIM Client using default resources
	- **Authentication:** A SCIM Client implementation with authenticated access to a Service Provider
	- **Configuration:** A SCIM Client implementation with custom HttpClient configuration
	- **Caching:** A SCIM Client implementation with custom caching mechanism for storing authentication tokens
- **ServiceProvider**
	- **In-Memory:** A basic implementation of a SCIM Service Provider using an in-memory store
	- **Extensions:** A SCIM Service Provider implementation utilizing the extensions feature of SCIM
	- **EntityFramework:** A SCIM Service Provider implementation with user data stored in a database
	- **AspNetIdentity:** A SCIM Service Provider implementation using ASP.NET Identity Store
	- **AuthenticationAndAuthorization:** A SCIM Service Provider implementation with the SCIM endpoints protected with authentication and authorization
	- **CustomStoreAndValidation:** A SCIM Service Provider implementation with custom stores and validation for SCIM requests

## Getting Started

- [Gettings Started article](https://www.identityserver.com/articles/managing-identities-across-cloud-based-applications-and-services-with-scim)
- [Documentation](https://www.identityserver.com/documentation/scim/)

## License Keys

For a demo license, please sign up on our [products page](https://www.identityserver.com/products/scim-for-aspnet), or reach out to <sales@identityserver.com>.
