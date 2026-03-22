namespace AutoRepository.Core;

/// <summary>
/// Full read/write repository interface for entities of type <typeparamref name="T"/>.
/// Inherits all read operations from <see cref="IReadRepository{T}"/> and adds write operations.
/// </summary>
/// <typeparam name="T">The entity type. Must be a reference type.</typeparam>
public interface IRepository<T> : IReadRepository<T>
    where T : class
{
    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities to the repository in a single operation.
    /// </summary>
    /// <param name="entities">The entities to add. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its primary key.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    /// <param name="id">The primary key value of the entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task DeleteByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities from the repository in a single operation.
    /// </summary>
    /// <param name="entities">The entities to delete. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}
