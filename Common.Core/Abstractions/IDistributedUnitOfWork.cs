namespace Common.Core.Abstractions;

public interface IDistributedUnitOfWork : IUnitOfWork
{
    Task<ITransaction> BeginDistributedTransactionAsync(CancellationToken cancellationToken = default);
}
