using System.Data;
using AutoRepository.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRepository.Dapper;

/// <summary>
/// Dapper implementation of <see cref="IUnitOfWork"/>.
/// Wraps a raw ADO.NET <see cref="IDbTransaction"/> for transaction management.
/// Use this when coordinating multiple repository operations across a single database transaction.
/// </summary>
public sealed class DapperUnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;

    /// <summary>
    /// Initializes a new instance of <see cref="DapperUnitOfWork"/>.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <exception cref="ArgumentNullException">Thrown if connection is null.</exception>
    public DapperUnitOfWork(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc />
    /// <remarks>
    /// For Dapper, there is no built-in change tracker, so this method does nothing.
    /// Changes are explicit via repository methods and must be committed manually.
    /// </remarks>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dapper has no change tracker — explicit commit is the save.
        return Task.FromResult(0);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown if a transaction is already in progress.</exception>
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        _transaction = _connection.BeginTransaction();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown if no transaction is in progress.</exception>
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}
