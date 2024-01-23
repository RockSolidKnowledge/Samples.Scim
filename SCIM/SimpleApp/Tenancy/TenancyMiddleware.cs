namespace SimpleApp.Tenancy;

internal class TenancyMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context , IScimTenancyContext tenancyContext)
    {
        // Check if the request path starts with the specified prefix

        if (context.Request.Path.StartsWithSegments("/scim",StringComparison.OrdinalIgnoreCase))
        {
            string[] pathParts = context.Request.Path.ToString().Split('/');
            
            string tenantId = pathParts[2];
            tenancyContext.SetTenantId(tenantId);
            
            context.Request.Path = new PathString("/scim/" + String.Join("/", pathParts[3..]));
        }

        // Call the next middleware in the pipeline
        await next(context);
    }
}