using System.Linq.Expressions;

namespace Common.Core.Abstractions;

/// <summary>
/// Specification pattern interface for building query specifications
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the filter criteria
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>
    /// Gets the list of include expressions
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the list of include strings
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the ordering expressions
    /// </summary>
    List<OrderByExpression<T>> OrderByExpressions { get; }

    /// <summary>
    /// Gets the paging parameters
    /// </summary>
    PaginationDto Pagination { get; }

    /// <summary>
    /// Gets whether the query should be tracked
    /// </summary>
    bool IsTracking { get; }

    /// <summary>
    /// Adds an include expression to the specification
    /// </summary>
    /// <param name="includeExpression">Include expression</param>
    ISpecification<T> Include(Expression<Func<T, object>> includeExpression);

    /// <summary>
    /// Adds an include string to the specification
    /// </summary>
    /// <param name="includeString">Include string</param>
    ISpecification<T> Include(string includeString);

    /// <summary>
    /// Adds an ordering expression to the specification
    /// </summary>
    /// <param name="orderByExpression">Ordering expression</param>
    /// <param name="isDescending">Whether to order descending</param>
    ISpecification<T> OrderBy(Expression<Func<T, object>> orderByExpression, bool isDescending = false);

    /// <summary>
    /// Applies paging to the specification
    /// </summary>
    /// <param name="pagination">Pagination parameters</param>
    ISpecification<T> Paginate(PaginationDto pagination);

    /// <summary>
    /// Sets whether the query should be tracked
    /// </summary>
    /// <param name="isTracking">Tracking flag</param>
    ISpecification<T> WithTracking(bool isTracking = true);
}

/// <summary>
/// Represents an ordering expression
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class OrderByExpression<T>
{
    public Expression<Func<T, object>> Expression { get; }
    public bool IsDescending { get; }

    public OrderByExpression(Expression<Func<T, object>> expression, bool isDescending = false)
    {
        Expression = expression;
        IsDescending = isDescending;
    }
}
