using AutoRepository.Core;
using Microsoft.EntityFrameworkCore;

namespace AutoRepository.EfCore;

/// <summary>
/// Default EF Core implementation of IRepository&lt;T&gt;.
/// Registered automatically — you never instantiate this directly.
/// </summary>
public class EfRepository<T> : IRepository<T>
    where T : class
{
    /// <summary>
    /// The DbContext instance used for database operations.
    /// </summary>
    protected readonly DbContext Context;

    /// <summary>
    /// The DbSet for the entity type.
    /// </summary>
    protected readonly DbSet<T> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfRepository{T}"/> class.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    public EfRepository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    // ── Reads ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets an entity by its primary key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    public async Task<T?> GetByIdAsync<TKey>(
        TKey id,
        CancellationToken cancellationToken = default
    ) => await DbSet.FindAsync(new object?[] { id }, cancellationToken);

    /// <summary>
    /// Gets all entities of type T.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An enumerable of all entities.</returns>
    public async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default
    ) => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    /// <summary>
    /// Gets entities that match the specification.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An enumerable of entities matching the specification.</returns>
    public async Task<IReadOnlyList<T>> GetAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) => await ApplySpecification(specification).AsNoTracking().ToListAsync(cancellationToken);

    /// <summary>
    /// Gets the first entity that matches the specification or null if none found.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first matching entity or null.</returns>
    public async Task<T?> GetFirstOrDefaultAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) =>
        await ApplySpecification(specification)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Gets a paginated set of entities matching the specification.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated result of entities.</returns>
    public async Task<PagedResult<T>> GetPagedAsync(
        ISpecification<T> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var baseQuery = ApplySpecification(specification, applyPaging: false);
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Counts entities that match the specification.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of matching entities.</returns>
    public async Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) => await ApplySpecification(specification).CountAsync(cancellationToken);

    /// <summary>
    /// Determines whether any entities match the specification.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if any entities match; otherwise false.</returns>
    public async Task<bool> AnyAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) => await ApplySpecification(specification).AnyAsync(cancellationToken);

    // ── Writes ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Adds multiple entities.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity by its primary key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task DeleteByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
    {
        var entity =
            await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Entity of type {typeof(T).Name} with id '{id}' was not found."
            );
        DbSet.Remove(entity);
    }

    /// <summary>
    /// Deletes multiple entities.
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task DeleteRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        DbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IQueryable<T> ApplySpecification(
        ISpecification<T> specification,
        bool applyPaging = true
    )
    {
        var spec = applyPaging ? specification : new NoPagingWrapper<T>(specification);
        return SpecificationEvaluator.GetQuery(DbSet.AsQueryable(), spec);
    }
}

/// <summary>Strips paging from a specification so we can count before slicing.</summary>
file sealed class NoPagingWrapper<T> : ISpecification<T>
{
    private readonly ISpecification<T> _inner;

    public NoPagingWrapper(ISpecification<T> inner) => _inner = inner;

    public System.Linq.Expressions.Expression<Func<T, bool>>? Criteria => _inner.Criteria;
    public List<System.Linq.Expressions.Expression<Func<T, object>>> Includes => _inner.Includes;
    public List<string> IncludeStrings => _inner.IncludeStrings;
    public System.Linq.Expressions.Expression<Func<T, object>>? OrderBy => _inner.OrderBy;
    public System.Linq.Expressions.Expression<Func<T, object>>? OrderByDescending =>
        _inner.OrderByDescending;
    public List<(
        System.Linq.Expressions.Expression<Func<T, object>> KeySelector,
        bool Descending
    )> ThenBys => _inner.ThenBys;
    public bool IsPagingEnabled => false;
    public int Skip => 0;
    public int Take => 0;
    public string? RawSql => _inner.RawSql;
    public object? RawSqlParameters => _inner.RawSqlParameters;
}
