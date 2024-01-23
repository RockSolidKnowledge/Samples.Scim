namespace SimpleApp.Tenancy;



public static class ScimTenancyExtensions
{
    public static IServiceCollection AddScimTenancy(this IServiceCollection services)
    {
        services.AddScoped<IScimTenancyAccessor,ScopedTenancyAccessor>();
        services.AddScoped<IScimTenancyContext>(provider => provider.GetRequiredService<IScimTenancyAccessor>() as ScopedTenancyAccessor);
        
        return services;
    }

    public static IApplicationBuilder UseScimTenancy(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenancyMiddleware>();
        return app;
    }
}