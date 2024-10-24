using System.Diagnostics.CodeAnalysis;
using Rsk.AspNetCore.Scim.Parsers;

public interface IScimPatchMap<TEntity> where TEntity : class
{
    bool TryGetPatchExecutor(PathExpression path, out IScimPatchOperationExecutor<TEntity>? patchMethod);
}

public class ScimPatchMap<TEntity> : IScimPatchMap<TEntity> where TEntity : class
{
    private readonly Dictionary<string, IScimPatchOperationExecutor<TEntity>> map;

    public ScimPatchMap(Dictionary<string, IScimPatchOperationExecutor<TEntity>> map)
    {
        this.map = map;
    }

    public bool TryGetPatchExecutor(PathExpression path, [NotNullWhen(true)] out IScimPatchOperationExecutor<TEntity>? patchMethod)
    {
        ArgumentNullException.ThrowIfNull(path);

        return map.TryGetValue(path.ToString(), out patchMethod);
    }
}