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
    protected readonly IDbConnection Connection;

    public DapperRepository(IDbConnection connection)
    {
        Connection = connection;
    }

    protected virtual string GetTableName() => typeof(T).Name + "s";

    protected virtual string GetIdColumnName() => "Id";

    // ── Reads ─────────────────────────────────────────────────────────────────

    public async Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {GetTableName()} WHERE {GetIdColumnName()} = @Id";
        return await Connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await Connection.QueryAsync<T>($"SELECT * FROM {GetTableName()}");
        return result.AsList();
    }

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

    public async Task<bool> AnyAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    ) => await CountAsync(specification, cancellationToken) > 0;

    // ── Writes ────────────────────────────────────────────────────────────────

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var properties = typeof(T).GetProperties().Where(p => p.Name != GetIdColumnName()).ToList();

        var columns = string.Join(", ", properties.Select(p => p.Name));
        var values = string.Join(", ", properties.Select(p => "@" + p.Name));
        var sql = $"INSERT INTO {GetTableName()} ({columns}) VALUES ({values})";

        await Connection.ExecuteAsync(sql, entity);
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var entity in entities)
            await AddAsync(entity, cancellationToken);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var properties = typeof(T).GetProperties().Where(p => p.Name != GetIdColumnName()).ToList();

        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
        var sql =
            $"UPDATE {GetTableName()} SET {setClause} WHERE {GetIdColumnName()} = @{GetIdColumnName()}";

        await Connection.ExecuteAsync(sql, entity);
    }

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

    public virtual async Task DeleteRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var entity in entities)
            await DeleteAsync(entity, cancellationToken);
    }
}
