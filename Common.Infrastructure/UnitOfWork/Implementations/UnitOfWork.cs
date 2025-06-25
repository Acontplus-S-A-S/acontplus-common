using Common.Infrastructure.Exceptions;
using Common.Infrastructure.Helpers;
using Common.Infrastructure.Repository.Implementations;
using Microsoft.EntityFrameworkCore.Storage; // Still needed here, as it's an implementation detail

namespace Common.Infrastructure.UnitOfWork.Implementations;

/// <summary>
/// Implements the Unit of Work pattern for managing database context and repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private readonly ILogger<UnitOfWork>? _logger;
    private readonly ConcurrentDictionary<Type, object> _repositories;
    private IDbContextTransaction? _currentTransaction; // This remains an internal field
    private bool _disposed = false;

    public UnitOfWork(DbContext context, ILogger<UnitOfWork>? logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
        _repositories = new ConcurrentDictionary<Type, object>();
    }

    /// <summary>
    /// Gets a repository instance for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>An instance of IRepository for the specified entity type.</returns>
    /// <exception cref="UnitOfWorkException">Thrown if repository creation fails.</exception>
    public IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : BaseEntity
    {
        var type = typeof(TEntity);
        if (!_repositories.ContainsKey(type))
        {
            try
            {
                var repository = new BaseRepository<TEntity>(_context, _logger as ILogger<BaseRepository<TEntity>>);
                _repositories.TryAdd(type, repository);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create repository for {EntityType}", typeof(TEntity).Name);
                throw new UnitOfWorkException($"Failed to create repository for {typeof(TEntity).Name}", ex);
            }
        }
        return (IRepository<TEntity>)_repositories[type];
    }

    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="UnitOfWorkException">Thrown if saving changes fails.</exception>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(SaveChangesAsync)}");
        try
        {
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving changes to the database.");
            throw new UnitOfWorkException("Error saving changes to the database.", ex);
        }
    }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <exception cref="UnitOfWorkException">Thrown if a transaction is already active or beginning fails.</exception>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) // Changed return type
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(BeginTransactionAsync)}");
        try
        {
            if (_currentTransaction != null)
            {
                throw new UnitOfWorkException("A transaction is already active.");
            }
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogInformation("Transaction started.");
            // No longer return _currentTransaction, it's managed internally
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error beginning transaction.");
            throw new UnitOfWorkException("Error beginning transaction.", ex);
        }
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <exception cref="UnitOfWorkException">Thrown if no transaction is active or committing fails.</exception>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(CommitAsync)}");
        try
        {
            if (_currentTransaction == null)
            {
                throw new UnitOfWorkException("No active transaction to commit.");
            }
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false); // Save changes before committing
            await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogInformation("Transaction committed successfully.");
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error committing transaction. Attempting rollback.");
            await RollbackAsync(cancellationToken).ConfigureAwait(false); // Rollback on commit failure
            throw new UnitOfWorkException("Error committing transaction.", ex);
        }
    }

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(RollbackAsync)}");
        try
        {
            if (_currentTransaction == null)
            {
                _logger?.LogWarning("No active transaction to rollback.");
                return;
            }
            await _currentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogInformation("Transaction rolled back.");
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error rolling back transaction.");
            throw new UnitOfWorkException("Error rolling back transaction.", ex);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            if (_currentTransaction != null)
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
            await _context.DisposeAsync().ConfigureAwait(false);
            _disposed = true;
        }
    }
}
