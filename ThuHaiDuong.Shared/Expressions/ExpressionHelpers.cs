using System.Linq.Expressions;

namespace ThuHaiDuong.Shared.Expressions;

public static class ExpressionHelpers
{
    public static Expression<Func<T, bool>> Combine<T, TValue>(
        Expression<Func<T, TValue>> propertySelector, 
        Expression<Func<TValue, bool>> predicate)
    {
        var parameter = propertySelector.Parameters[0];
        
        var visitor = new ReplaceExpressionVisitor(predicate.Parameters[0], propertySelector.Body);
        var body = visitor.Visit(predicate.Body);
        
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}