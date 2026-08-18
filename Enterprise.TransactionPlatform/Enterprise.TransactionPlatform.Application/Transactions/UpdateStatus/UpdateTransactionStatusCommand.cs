using Enterprise.TransactionPlatform.Domain.Enums;

namespace Enterprise.TransactionPlatform.Application.Transactions.UpdateStatus
{
    public sealed record UpdateTransactionStatusCommand(Guid TransactionId, TransactionStatus Status);
}
