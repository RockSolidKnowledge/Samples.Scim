using System.Linq.Expressions;
using System.Reflection;
using Rsk.AspNetCore.Scim.Parsers;
using Rsk.AspNetCore.Scim.Stores;

public interface IScimPatchOperationExecutor<TEntity> where TEntity: class
{
    void Execute(TEntity target, PatchCommand command);
}

public class ScimPatchOperationExecutor<TEntity> : IScimPatchOperationExecutor<TEntity> where TEntity: class
{
    private readonly PropertyInfo property;
    private LiteralConverter? converter;

    public ScimPatchOperationExecutor(
        MemberExpression memberExpression,
        LiteralConverter? converter)
    {
        MemberInfo member = memberExpression.Member;
        if (member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Member must be a property", nameof(memberExpression));
        }

        if (propertyInfo.GetSetMethod() == null)
        {
            throw new ArgumentException("Member must have a set method", nameof(memberExpression));
        }

        property = propertyInfo;
        this.converter = converter;
    }

    public void Execute(TEntity target, PatchCommand command)
    {
        property.SetValue(target, converter != null ? converter(command.Value) : command.Value);
    }
}