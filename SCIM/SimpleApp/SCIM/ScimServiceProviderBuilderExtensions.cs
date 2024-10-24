using Microsoft.Extensions.DependencyInjection.Extensions;
using Rsk.AspNetCore.Scim.Configuration.DependencyInjection;

namespace SimpleApp.SCIM;

public static class ScimServiceProviderBuilderExtensions
{
    public static IScimServiceProviderBuilder MapPatchingOperations<TEntity>(this IScimServiceProviderBuilder builder, string schema, Action<IScimPatchMapBuilder<TEntity>> configure)
        where TEntity : class
    {
        ScimPatchMapBuilder<TEntity> mapBuilder = new ScimPatchMapBuilder<TEntity>(schema);

        configure(mapBuilder);

        IScimPatchMap<TEntity> map = mapBuilder.Build();

        builder.Services.TryAddSingleton<IScimPatcher<TEntity>, ScimPatcher<TEntity>>();
        builder.Services.AddSingleton(map);

        return builder;
    }
}