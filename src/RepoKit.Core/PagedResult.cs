namespace AutoRepository.Core;

/// <summary>
/// Encapsulates a page of query results with pagination metadata.
/// Returned by <see cref="IReadRepository{T}.GetPagedAsync"/>.
/// </summary>
/// <typeparam name="T">The type of items in the result page.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Gets the items on this page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates a new <see cref="PagedResult{T}"/> with the specified items and pagination metadata.
    /// </summary>
    /// <param name="items">The items on this page. Must not be null.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A new <see cref="PagedResult{T}"/> instance.</returns>
    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize
    ) =>
        new()
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

    /// <summary>
    /// Creates an empty <see cref="PagedResult{T}"/> with no items.
    /// Useful for queries that return no results.
    /// </summary>
    /// <param name="pageNumber">The page number for empty results. Defaults to 1.</param>
    /// <param name="pageSize">The page size. Defaults to 10.</param>
    /// <returns>An empty <see cref="PagedResult{T}"/> instance.</returns>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10) =>
        new() { PageNumber = pageNumber, PageSize = pageSize };
}

/// <summary>
/// Unit of Work pattern implementation for coordinating changes across multiple repositories in a single transaction.
/// Inject this when you need to save changes across multiple entities atomically.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Saves all pending changes to the underlying data store.
    /// For EF Core, this persists pending additions, modifications, and deletions.
    /// For Dapper, this is a no-op as changes are explicit.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The number of entities affected, or 0 if no changes were pending.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// All subsequent repository operations will be part of this transaction until committed or rolled back.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction, persisting all changes atomically.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no transaction is in progress.</exception>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction, discarding all unpersisted changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
