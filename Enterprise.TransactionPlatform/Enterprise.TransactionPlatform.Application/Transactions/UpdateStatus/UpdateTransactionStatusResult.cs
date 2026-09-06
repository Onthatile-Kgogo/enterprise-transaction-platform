using Enterprise.TransactionPlatform.Domain.Enums;

namespace Enterprise.TransactionPlatform.Application.Transactions.UpdateStatus
{
    public sealed record UpdateTransactionStatusResult(Guid TransactionId, string Reference, TransactionStatus Status, DateTime? UpdatedAtUtc);
}
