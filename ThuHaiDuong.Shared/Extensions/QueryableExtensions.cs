using System.Linq.Expressions;
using ThuHaiDuong.Shared.Expressions;

namespace ThuHaiDuong.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyNumericFilter<T>(
        this IQueryable<T> query, 
        Expression<Func<T, decimal?>> propertySelector, 
        string? op, 
        decimal? value)
    {
        if (!value.HasValue) return query;

        Expression<Func<decimal?, bool>> predicate = op?.ToLower() switch
        {
            "lt" or "less than" => v => v < value,
            "gt" or "greater than" => v => v > value,
            "neq" or "not equal" => v => v != value,
            _ => v => v == value
        };

        return query.Where(ExpressionHelpers.Combine(propertySelector, predicate));
    }

    public static IQueryable<T> ApplyDateFilter<T>(
        this IQueryable<T> query, 
        Expression<Func<T, DateTime?>> propertySelector, 
        string? op, 
        DateTime? value)
    {
        if (!value.HasValue) return query;
        var dateValue = value.Value.Date;

        Expression<Func<DateTime?, bool>> predicate = op?.ToLower() switch
        {
            "before" => v => v < dateValue,
            "after"  => v => v > dateValue.AddDays(1).AddTicks(-1),
            "on"     => v => v >= dateValue && v < dateValue.AddDays(1),
            _ => v => v == dateValue
        };

        return query.Where(ExpressionHelpers.Combine(propertySelector, predicate));
    }
    
    public static IQueryable<T> ApplyCollectionDateFilter<T, TCollection>(
        this IQueryable<T> query,
        Expression<Func<T, IEnumerable<TCollection>>> collectionSelector,
        Expression<Func<TCollection, DateTime?>> datePropertySelector,
        string? op,
        DateTime? value)
    {
        if (!value.HasValue) return query;
        var dateValue = value.Value.Date;

        Expression<Func<DateTime?, bool>> datePredicate = op?.ToLower() switch
        {
            "before" => v => v < dateValue,
            "after"  => v => v > dateValue.AddDays(1).AddTicks(-1),
            "on"     => v => v >= dateValue && v < dateValue.AddDays(1),
            _        => v => v == dateValue
        };

        var innerExpression = ExpressionHelpers.Combine(datePropertySelector, datePredicate);

        var parameter = collectionSelector.Parameters[0];
        var anyMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TCollection));

        var anyCall = Expression.Call(anyMethod, collectionSelector.Body, innerExpression);
        var lambda = Expression.Lambda<Func<T, bool>>(anyCall, parameter);

        return query.Where(lambda);
    }
}