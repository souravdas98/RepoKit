using AutoRepository.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoRepository.EfCore;

/// <summary>
/// Entity Framework Core implementation of <see cref="IUnitOfWork"/>.
/// Wraps the DbContext transaction lifecycle and change tracking.
/// Inject this when you need atomic multi-repository operations or explicit transaction control.
/// </summary>
public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Initializes a new instance of <see cref="EfUnitOfWork"/>.
    /// </summary>
    /// <param name="context">The Entity Framework DbContext. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if context is null.</exception>
    public EfUnitOfWork(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    /// <remarks>
    /// This persists all pending changes (additions, modifications, deletions) tracked by the DbContext.
    /// </remarks>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown if a transaction is already in progress.</exception>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException(
                "A transaction is already in progress. Complete or rollback the current transaction before starting a new one."
            );
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown if no transaction is in progress.</exception>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException(
                "No transaction in progress. Call BeginTransactionAsync first."
            );

        await _context.SaveChangesAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            return;
        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}
