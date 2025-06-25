using System.Data.Common;
using Common.Infrastructure.Exceptions;
using Common.Infrastructure.Repository.Implementations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Common.Infrastructure.UnitOfWork.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private readonly IAdoRepository _adoRepository;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly ConcurrentDictionary<Type, object> _repositories;
    private IDbContextTransaction _efTransaction;
    private bool _disposed = false;

    public UnitOfWork(DbContext context, IAdoRepository adoRepository, ILogger<UnitOfWork> logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _adoRepository = adoRepository ?? throw new ArgumentNullException(nameof(adoRepository));
        _logger = logger;
        _repositories = new ConcurrentDictionary<Type, object>();
    }

    public DbTransaction CurrentDbTransaction => _efTransaction?.GetDbTransaction();
    public DbConnection CurrentDbConnection => _context.Database.GetDbConnection();
    public IAdoRepository AdoRepository => _adoRepository;

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity);
        return (IRepository<TEntity>)_repositories.GetOrAdd(type, _ =>
            new BaseRepository<TEntity>(_context, _logger as ILogger<BaseRepository<TEntity>>));
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_efTransaction != null) return;

        try
        {
            // Ensure connection is open
            var connection = CurrentDbConnection;
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            // Begin EF transaction
            _efTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            // Share transaction with ADO repository
            _adoRepository.SetTransaction(CurrentDbTransaction);
            _adoRepository.SetConnection(connection);

            _logger?.LogInformation("Transaction started and shared with ADO repository");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error beginning transaction");
            throw new UnitOfWorkException("Error beginning transaction", ex);
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_efTransaction == null)
            throw new UnitOfWorkException("No active transaction to commit");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _efTransaction.CommitAsync(cancellationToken);
            _logger?.LogInformation("Transaction committed successfully");
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await CleanupTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_efTransaction == null)
        {
            _logger?.LogWarning("Rollback called with no active transaction");
            return;
        }

        try
        {
            await _efTransaction.RollbackAsync(cancellationToken);
            _logger?.LogInformation("Transaction rolled back");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error rolling back transaction");
            throw new UnitOfWorkException("Error rolling back transaction", ex);
        }
        finally
        {
            await CleanupTransactionAsync();
        }
    }

    private async Task CleanupTransactionAsync()
    {
        if (_efTransaction != null)
        {
            _adoRepository.ClearTransaction();
            await _efTransaction.DisposeAsync();
            _efTransaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving changes");
            throw new UnitOfWorkException("Error saving changes", ex);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _efTransaction?.Dispose();
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
            if (_efTransaction != null)
            {
                await _efTransaction.DisposeAsync();
            }
            await _context.DisposeAsync();
            _disposed = true;
        }
    }
}
