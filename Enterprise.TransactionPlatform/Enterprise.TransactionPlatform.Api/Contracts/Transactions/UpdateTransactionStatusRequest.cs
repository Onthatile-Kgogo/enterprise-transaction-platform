using Enterprise.TransactionPlatform.Domain.Enums;

namespace Enterprise.TransactionPlatform.Api.Contracts.Transactions
{
    public sealed record UpdateTransactionStatusRequest(TransactionStatus Status);
}
