namespace AutoRepository.Core;

/// <summary>
/// Read-only repository interface for entities of type <typeparamref name="T"/>.
/// Provides async query operations without write capabilities.
/// Use this when you want to prevent accidental writes from a consumer.
/// </summary>
/// <typeparam name="T">The entity type. Must be a reference type.</typeparam>
public interface IReadRepository<T>
    where T : class
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A read-only list of all entities.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries entities using a specification with filtering, ordering, and eager loading.
    /// </summary>
    /// <param name="specification">The specification containing query criteria, includes, and ordering.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A read-only list of entities matching the specification.</returns>
    Task<IReadOnlyList<T>> GetAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the first entity matching the specification, or null if none found.
    /// </summary>
    /// <param name="specification">The specification containing query criteria, includes, and ordering.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The first matching entity, or null if not found.</returns>
    Task<T?> GetFirstOrDefaultAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a page of entities matching the specification.
    /// </summary>
    /// <param name="specification">The specification containing query criteria and ordering.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A <see cref="PagedResult{T}"/> containing the page of entities and pagination metadata.</returns>
    Task<PagedResult<T>> GetPagedAsync(
        ISpecification<T> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts the number of entities matching the specification.
    /// </summary>
    /// <param name="specification">The specification containing query criteria.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The count of matching entities.</returns>
    Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether any entity matches the specification.
    /// </summary>
    /// <param name="specification">The specification containing query criteria.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>True if at least one entity matches; otherwise, false.</returns>
    Task<bool> AnyAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    );
}
