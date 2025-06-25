using System.Data.Common;
using Common.Core.Base;

namespace Common.Core.Abstractions;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;
    IAdoRepository AdoRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);

    DbTransaction CurrentDbTransaction { get; }
    DbConnection CurrentDbConnection { get; }
}
