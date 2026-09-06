using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;
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

        public static Transaction ToDomain(TransactionRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);

            var type = Enum.Parse<TransactionType>(
                record.Type,
                ignoreCase: true);

            var status = Enum.Parse<TransactionStatus>(
                record.Status,
                ignoreCase: true);

            return Transaction.Rehydrate(
                transactionId: record.TransactionId,
                reference: record.Reference,
                amount: record.Amount,
                currency: record.Currency,
                type: type,
                status: status,
                description: record.Description,
                createdAtUtc: record.CreatedAtUtc,
                updatedAtUtc: record.UpdatedAtUtc);
        }
    }
}
