using Common.Core.Base;

namespace Common.Core.Abstractions;

/// <summary>
/// Defines the contract for a Unit of Work, coordinating repository operations and managing transactions.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets a repository instance for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>An instance of IRepository for the specified entity type.</returns>
    IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : BaseEntity;

    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default); // Changed return type

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
