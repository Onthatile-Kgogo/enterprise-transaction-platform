using Enterprise.TransactionPlatform.Domain.Entities;

namespace Enterprise.TransactionPlatform.Application.Abstractions.Persistence
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
        Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
        Task<Transaction?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(Transaction transaction, CancellationToken cancellationToken = default);
    }
}
