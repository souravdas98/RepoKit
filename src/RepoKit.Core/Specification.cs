using System.Linq.Expressions;

namespace AutoRepository.Core;

/// <summary>
/// Encapsulates a query specification for filtering, ordering, and eager loading entities.
/// Supports both LINQ (EF Core) and raw SQL (Dapper) approaches.
/// Implement this interface or inherit from <see cref="BaseSpecification{T}"/> to build type-safe queries.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the WHERE clause filter as a LINQ expression.
    /// Null means no filtering.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the collection of navigation properties to eagerly load (for EF Core).
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the collection of navigation properties to eagerly load by string path (for EF Core).
    /// Example: "Orders", "Orders.Items"
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the primary ORDER BY clause (ascending).
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the primary ORDER BY clause (descending).
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Gets additional ORDER BY clauses (THEN BY) with their sort direction.
    /// </summary>
    List<(Expression<Func<T, object>> KeySelector, bool Descending)> ThenBys { get; }

    /// <summary>
    /// Gets a value indicating whether pagination is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Gets the number of records to skip (used with paging).
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Gets the number of records to take (used with paging).
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Gets raw SQL for the query (Dapper-specific).
    /// When set, Dapper will execute this instead of building a query from other properties.
    /// </summary>
    string? RawSql { get; }

    /// <summary>
    /// Gets parameters for the raw SQL query (Dapper-specific).
    /// </summary>
    object? RawSqlParameters { get; }
}

/// <summary>
/// Base class for building type-safe specifications.
/// Inherit from this class and call protected methods in the constructor to build a query.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Initializes a new instance of <see cref="BaseSpecification{T}"/> without any initial criteria.
    /// </summary>
    protected BaseSpecification() { }

    /// <summary>
    /// Initializes a new instance of <see cref="BaseSpecification{T}"/> with an initial WHERE clause.
    /// </summary>
    /// <param name="criteria">The initial filter expression.</param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <inheritdoc />
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc />
    public List<Expression<Func<T, object>>> Includes { get; } = new();

    /// <inheritdoc />
    public List<string> IncludeStrings { get; } = new();

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc />
    public List<(Expression<Func<T, object>> KeySelector, bool Descending)> ThenBys { get; } =
        new();

    /// <inheritdoc />
    public bool IsPagingEnabled { get; private set; }

    /// <inheritdoc />
    public int Skip { get; private set; }

    /// <inheritdoc />
    public int Take { get; private set; }

    /// <inheritdoc />
    public string? RawSql { get; private set; }

    /// <inheritdoc />
    public object? RawSqlParameters { get; private set; }

    /// <summary>
    /// Sets the WHERE clause filter.
    /// </summary>
    /// <param name="criteria">The filter expression.</param>
    protected void AddCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    /// <summary>
    /// Adds a navigation property to eagerly load (LINQ include).
    /// </summary>
    /// <param name="include">The navigation property expression.</param>
    protected void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);

    /// <summary>
    /// Adds a navigation property to eagerly load by string path.
    /// Example: AddInclude("Orders"), AddInclude("Orders.Items")
    /// </summary>
    /// <param name="include">The navigation property path.</param>
    protected void AddInclude(string include) => IncludeStrings.Add(include);

    /// <summary>
    /// Sets the primary ORDER BY clause (ascending).
    /// </summary>
    /// <param name="orderBy">The ordering expression.</param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;

    /// <summary>
    /// Sets the primary ORDER BY clause (descending).
    /// </summary>
    /// <param name="orderByDesc">The descending ordering expression.</param>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDesc) =>
        OrderByDescending = orderByDesc;

    /// <summary>
    /// Adds a secondary ORDER BY (THEN BY) clause.
    /// </summary>
    /// <param name="keySelector">The ordering expression.</param>
    /// <param name="descending">Whether to sort in descending order. Defaults to false (ascending).</param>
    protected void AddThenBy(Expression<Func<T, object>> keySelector, bool descending = false) =>
        ThenBys.Add((keySelector, descending));

    /// <summary>
    /// Enables pagination with the specified skip and take values.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Sets a raw SQL query (Dapper-specific).
    /// When set, the Dapper repository will execute this raw SQL instead of evaluating LINQ expressions.
    /// The EF Core implementation may fall back to LINQ if available.
    /// </summary>
    /// <param name="sql">The raw SQL query string.</param>
    /// <param name="parameters">Optional parameters for parameterized queries.</param>
    protected void UseRawSql(string sql, object? parameters = null)
    {
        RawSql = sql;
        RawSqlParameters = parameters;
    }
}
