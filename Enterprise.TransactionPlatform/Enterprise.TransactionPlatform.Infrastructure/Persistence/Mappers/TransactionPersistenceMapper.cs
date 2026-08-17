using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Infrastructure.Persistence.Models;

namespace Enterprise.TransactionPlatform.Infrastructure.Persistence.Mappers
{
    internal static class TransactionPersistenceMapper
    {
        public static TransactionRecord ToRecord(Transaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            return new TransactionRecord
            {
                TransactionId = transaction.TransactionId,
                Reference = transaction.Reference.Value,
                Amount = transaction.Money.Amount,
                Currency = transaction.Money.Currency.Code,
                Type = transaction.Type.ToString(),
                Status = transaction.Status.ToString(),
                Description = transaction.Description,
                CreatedAtUtc = transaction.CreatedAtUtc,
                UpdatedAtUtc = transaction.UpdatedAtUtc
            };
        }
    }
}
