
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Rsk.AspNetCore.Scim.Parsers;

public interface IScimPatchMapBuilder<TEntity> where TEntity : class
{
    ScimPatchMapBuilder<TEntity> Map<TProperty>(string attribute,
        Expression<Func<TEntity, TProperty>> propertyAccessor,
        LiteralConverter converter = null);
}

public class ScimPatchMapBuilder<TEntity> : IScimPatchMapBuilder<TEntity>
    where TEntity : class
{
    private string schema;
    private Dictionary<string, IScimPatchOperationExecutor<TEntity>> map = new ();

    public ScimPatchMapBuilder(string schema)
    {
        this.schema = schema;
    }

    public ScimPatchMapBuilder<TEntity> Map<TResult>(string attribute,
        Expression<Func<TEntity, TResult>> propertyAccessor,
        LiteralConverter converter = null)
    {
        map.Add(attribute, new ScimPatchOperationExecutor<TEntity>(propertyAccessor.Body as MemberExpression, converter));

        return this;
    }

    public IScimPatchMap<TEntity> Build()
    {
        return new ScimPatchMap<TEntity>(schema, map);
    }
}

public interface IScimPatchMap<TEntity> where TEntity : class
{
    bool TryGetPatchMethod(PathExpression path, out IScimPatchOperationExecutor<TEntity>? patchMethod);
}

public class ScimPatchMap<TEntity> : IScimPatchMap<TEntity> where TEntity : class
{
    private readonly string schema;
    private readonly Dictionary<string, IScimPatchOperationExecutor<TEntity>> map;

    public ScimPatchMap(string schema, Dictionary<string, IScimPatchOperationExecutor<TEntity>> map)
    {
        this.schema = schema;
        this.map = map;
    }

    public bool TryGetPatchMethod(PathExpression path, [NotNullWhen(true)] out IScimPatchOperationExecutor<TEntity>? patchMethod)
    {
        // Visit the path expression to match the nearest path?
        return map.TryGetValue(path.ToString(), out patchMethod);
    }
}