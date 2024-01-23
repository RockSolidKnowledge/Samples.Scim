using Rsk.AspNetCore.Scim.Parsers;

namespace SimpleApp;


public class DecompileFilter : FilterExpressionVisitor<string>
{
    public override string Visit(AttributePathExpression expression)
    {
        return $"{String.Join('.', expression.PathElements)}";
    }
    

    public override string Visit(AlwaysTrueFilterExpression expression)
    {
        return "true";
    }

    public override string Visit(LiteralFilterExpression<string> expression)
    {
        return base.Visit(expression);
    }

    public override string Visit(LiteralNullFilterExpression expression)
    {
        return String.Empty;
    }

    public override string Visit(LiteralStringFilterExpression expression)
    {
        return $"'{expression.Value}'";
    }

    public override string Visit(AttributeComparisonFilterExpression expression)
    {
        return $"{expression.Attribute} {expression.Operator} {expression.Literal.Accept(this)}";
    }

    public override string Visit(AndFilterExpression expression)
    {
        return $"({expression.Lhs.Accept(this)} and {expression.Rhs.Accept(this)})";
    }
    
    public override string Visit(OrFilterExpression expression)
    {
        return $"({expression.Lhs.Accept(this)} or {expression.Rhs.Accept(this)})";
    }
    
    public override string Visit(NotFilterExpression expression)
    {
        return $"not {expression.Expression.Accept(this)}";
    }

    public override string Visit(LiteralFilterExpression<bool> expression)
    {
        return expression.Value.ToString().ToLower();
    }

    public override string Visit(LiteralFilterExpression<int> expression)
    {
        return expression.Value.ToString();
    }
    
    public override string Visit(LiteralFilterExpression<decimal> expression)
    {
        return expression.Value.ToString();
    }

    public override string Visit(ValuePathExpression expression)
    {
        return $"{String.Join( '.' , expression.PathElements)}[{expression.ValueFilter.Accept(this)}]";
    }
}
