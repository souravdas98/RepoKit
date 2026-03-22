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
    protected readonly DbContext Context;
    protected readonly DbSet<T> DbSet;

    public EfRepository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    // ── Reads ────────────────────────────────────────────────────────────────

    public async Task<T?> GetByIdAsync<TKey>(
        TKey id,
        CancellationToken cancellationToken = default
    ) => await DbSet.FindAsync(new object?[] { id }, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default
    ) => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> GetAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) => await ApplySpecification(specification).AsNoTracking().ToListAsync(cancellationToken);

    public async Task<T?> GetFirstOrDefaultAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) =>
        await ApplySpecification(specification)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

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

    public async Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) => await ApplySpecification(specification).CountAsync(cancellationToken);

    public async Task<bool> AnyAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) => await ApplySpecification(specification).AnyAsync(cancellationToken);

    // ── Writes ───────────────────────────────────────────────────────────────

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
    {
        var entity =
            await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Entity of type {typeof(T).Name} with id '{id}' was not found."
            );
        DbSet.Remove(entity);
    }

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
