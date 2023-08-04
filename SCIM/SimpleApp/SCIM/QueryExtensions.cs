using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Parsers;

namespace SimpleApp.SCIM;

public static class QueryExtensions
{
    private static readonly AttributePathExpression ExternalId =
        new AttributePathExpression(ScimSchemas.User, "externalId");
    
    public static bool IsExternalIdEqualityExpression(this ScimExpression expression, out string? idToMatch)
    {
        idToMatch = null;
        if (( expression is AttributeComparisonFilterExpression { Operator: AttributeComparisonOperators.Equal } attributeFilterComparison  ))
        {
            if (attributeFilterComparison.Attribute.Equals(ExternalId))
            {
                if (attributeFilterComparison.Literal is LiteralStringFilterExpression externalId)
                {
                    idToMatch = externalId.Value;
                    return true;
                }
            }
        }

        return false;
    }
}