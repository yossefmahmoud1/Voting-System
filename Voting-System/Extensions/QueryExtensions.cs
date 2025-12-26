using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VotingSystem.Dtos.Common;

namespace VotingSystem.Extensions;

public static class QueryExtensions
{
    /// <summary>
    /// Applies sorting to the query based on SortColumn and SortDirection
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sortColumn,
        string? sortDirection = "Asc")
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            return query;

        var propertyInfo = typeof(T).GetProperty(
            sortColumn,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyInfo);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = string.Equals(sortDirection, "Desc", StringComparison.OrdinalIgnoreCase)
            ? "OrderByDescending"
            : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), propertyInfo.PropertyType },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    /// <summary>
    /// Applies search filter to string properties of the entity
    /// </summary>
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? searchValue,
        params Expression<Func<T, string>>[] searchProperties)
    {
        if (string.IsNullOrWhiteSpace(searchValue) || searchProperties.Length == 0)
            return query;

        var searchLower = searchValue.ToLower().Trim();
        var parameter = Expression.Parameter(typeof(T), "x");

        Expression? combinedExpression = null;

        foreach (var propertyExpression in searchProperties)
        {
            // Get the property access expression
            var propertyAccess = propertyExpression.Body;
            
            // Handle nullable string properties
            if (propertyAccess.Type == typeof(string))
            {
                // Convert to lowercase and check Contains
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var constant = Expression.Constant(searchLower, typeof(string));

                // Handle null-safe: x.Property != null && x.Property.ToLower().Contains(searchValue)
                var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));
                var toLowerCall = Expression.Call(propertyAccess, toLowerMethod!);
                var containsCall = Expression.Call(toLowerCall, containsMethod!, constant);
                var andExpression = Expression.AndAlso(nullCheck, containsCall);

                if (combinedExpression == null)
                    combinedExpression = andExpression;
                else
                    combinedExpression = Expression.OrElse(combinedExpression, andExpression);
            }
        }

        if (combinedExpression == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Applies pagination, sorting, and search to a query
    /// </summary>
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        RequestFilters filters,
        params Expression<Func<T, string>>[] searchProperties)
    {
        // Apply search first
        query = query.ApplySearch(filters.SearchValue, searchProperties);

        // Apply sorting
        query = query.ApplySorting(filters.SortColumn, filters.SortDirection);

        return query;
    }
}
