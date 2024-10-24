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
    private readonly string schema;
    private Dictionary<string, IScimPatchOperationExecutor<TEntity>> map = new ();

    public ScimPatchMapBuilder(string schema)
    {
        this.schema = schema;
    }

    public ScimPatchMapBuilder<TEntity> Map<TResult>(string attribute,
        Expression<Func<TEntity, TResult>> propertyAccessor,
        LiteralConverter converter = null)
    {
        map.Add($"{schema}:{attribute}", new ScimPatchOperationExecutor<TEntity>(propertyAccessor.Body as MemberExpression, converter));

        return this;
    }

    public IScimPatchMap<TEntity> Build()
    {
        return new ScimPatchMap<TEntity>(map);
    }
}