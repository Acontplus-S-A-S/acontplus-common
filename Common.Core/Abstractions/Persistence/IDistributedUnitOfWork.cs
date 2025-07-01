namespace Common.Core.Abstractions.Persistence;

public interface IDistributedUnitOfWork : IUnitOfWork
{
    Task<ITransaction> BeginDistributedTransactionAsync(CancellationToken cancellationToken = default);
}
