using System.Data;
using AutoRepository.Core;
using Dapper;

namespace AutoRepository.Dapper;

/// <summary>
/// Dapper implementation of IRepository&lt;T&gt;.
/// Use when you want raw SQL performance for a specific entity.
///
/// Override GetTableName() if your table name differs from the class name.
/// Override GetIdColumnName() if your PK column isn't "Id".
///
/// For complex queries, set RawSql in your specification.
/// </summary>
public class DapperRepository<T> : IRepository<T>
    where T : class
{
    /// <summary>
    /// Gets the database connection used for executing SQL commands.
    /// </summary>
    protected readonly IDbConnection Connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperRepository{T}"/> class.
    /// </summary>
    /// <param name="connection">The database connection to use for queries.</param>
    public DapperRepository(IDbConnection connection)
    {
        Connection = connection;
    }

    /// <summary>
    /// Gets the table name for the entity. Override to customize table naming.
    /// </summary>
    /// <returns>The table name (default: class name + 's').</returns>
    protected virtual string GetTableName() => typeof(T).Name + "s";

    /// <summary>
    /// Gets the identity column name for the entity. Override to customize primary key naming.
    /// </summary>
    /// <returns>The identity column name (default: "Id").</returns>
    protected virtual string GetIdColumnName() => "Id";

    // ── Reads ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retrieves a single entity by its primary key.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public async Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {GetTableName()} WHERE {GetIdColumnName()} = @Id";
        return await Connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    /// <summary>
    /// Retrieves all entities from the table.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of all entities.</returns>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await Connection.QueryAsync<T>($"SELECT * FROM {GetTableName()}");
        return result.AsList();
    }

    /// <summary>
    /// Retrieves entities matching the specified criteria.
    /// </summary>
    /// <param name="specification">The specification containing RawSql for filtering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of matching entities.</returns>
    /// <exception cref="NotSupportedException">Thrown when RawSql is not provided in the specification.</exception>
    public async Task<IReadOnlyList<T>> GetAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    )
    {
        if (specification.RawSql is not null)
        {
            var result = await Connection.QueryAsync<T>(
                specification.RawSql,
                specification.RawSqlParameters
            );
            return result.AsList();
        }

        // Fallback: simple WHERE from criteria (Dapper can't evaluate LINQ trees natively)
        // For complex cases, always use RawSql in your specification.
        throw new NotSupportedException(
            $"Dapper repository for '{typeof(T).Name}' requires a RawSql specification for filtered queries. "
                + $"Use spec.UseRawSql(\"SELECT * FROM ...\", parameters) in your specification."
        );
    }

    /// <summary>
    /// Retrieves the first entity matching the specified criteria.
    /// </summary>
    /// <param name="specification">The specification containing RawSql for filtering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first matching entity if found; otherwise, null.</returns>
    /// <exception cref="NotSupportedException">Thrown when RawSql is not provided in the specification.</exception>
    public async Task<T?> GetFirstOrDefaultAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    )
    {
        if (specification.RawSql is not null)
            return await Connection.QueryFirstOrDefaultAsync<T>(
                specification.RawSql,
                specification.RawSqlParameters
            );

        throw new NotSupportedException(
            "Dapper repository requires RawSql in the specification for filtered queries."
        );
    }

    /// <summary>
    /// Retrieves a paginated set of entities matching the specified criteria.
    /// </summary>
    /// <param name="specification">The specification containing RawSql for filtering.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the entities and total count.</returns>
    /// <exception cref="NotSupportedException">Thrown when RawSql is not provided in the specification.</exception>
    public async Task<PagedResult<T>> GetPagedAsync(
        ISpecification<T> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        if (specification.RawSql is null)
            throw new NotSupportedException(
                "Dapper paged queries require RawSql in the specification."
            );

        var offset = (pageNumber - 1) * pageSize;
        var pagedSql =
            $"{specification.RawSql} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
        var countSql = $"SELECT COUNT(*) FROM ({specification.RawSql}) AS _count";

        var items = (
            await Connection.QueryAsync<T>(pagedSql, specification.RawSqlParameters)
        ).AsList();
        var totalCount = await Connection.ExecuteScalarAsync<int>(
            countSql,
            specification.RawSqlParameters
        );

        return PagedResult<T>.Create(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Counts entities matching the specified criteria.
    /// </summary>
    /// <param name="specification">The specification containing optional RawSql for filtering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of matching entities.</returns>
    public async Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    )
    {
        if (specification.RawSql is not null)
        {
            var countSql = $"SELECT COUNT(*) FROM ({specification.RawSql}) AS _count";
            return await Connection.ExecuteScalarAsync<int>(
                countSql,
                specification.RawSqlParameters
            );
        }

        return await Connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {GetTableName()}");
    }

    /// <summary>
    /// Checks if any entities match the specified criteria.
    /// </summary>
    /// <param name="specification">The specification containing optional RawSql for filtering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if at least one entity matches; otherwise, false.</returns>
    public async Task<bool> AnyAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) => await CountAsync(specification, cancellationToken) > 0;

    // ── Writes ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a new entity to the table.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var properties = typeof(T).GetProperties().Where(p => p.Name != GetIdColumnName()).ToList();

        var columns = string.Join(", ", properties.Select(p => p.Name));
        var values = string.Join(", ", properties.Select(p => "@" + p.Name));
        var sql = $"INSERT INTO {GetTableName()} ({columns}) VALUES ({values})";

        await Connection.ExecuteAsync(sql, entity);
    }

    /// <summary>
    /// Adds multiple entities to the table.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var entity in entities)
            await AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Updates an existing entity in the table.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var properties = typeof(T).GetProperties().Where(p => p.Name != GetIdColumnName()).ToList();

        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
        var sql =
            $"UPDATE {GetTableName()} SET {setClause} WHERE {GetIdColumnName()} = @{GetIdColumnName()}";

        await Connection.ExecuteAsync(sql, entity);
    }

    /// <summary>
    /// Deletes an entity from the table.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        var idProp =
            typeof(T).GetProperty(GetIdColumnName())
            ?? throw new InvalidOperationException(
                $"Entity '{typeof(T).Name}' has no property named '{GetIdColumnName()}'."
            );
        var id = idProp.GetValue(entity);
        await Connection.ExecuteAsync(
            $"DELETE FROM {GetTableName()} WHERE {GetIdColumnName()} = @Id",
            new { Id = id }
        );
    }

    /// <summary>
    /// Deletes an entity by its primary key.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task DeleteByIdAsync<TKey>(
        TKey id,
        CancellationToken cancellationToken = default
    )
    {
        await Connection.ExecuteAsync(
            $"DELETE FROM {GetTableName()} WHERE {GetIdColumnName()} = @Id",
            new { Id = id }
        );
    }

    /// <summary>
    /// Deletes multiple entities from the table.
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task DeleteRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var entity in entities)
            await DeleteAsync(entity, cancellationToken);
    }
}
