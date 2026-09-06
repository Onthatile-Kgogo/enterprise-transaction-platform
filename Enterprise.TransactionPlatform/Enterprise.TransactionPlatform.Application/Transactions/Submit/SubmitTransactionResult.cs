using Enterprise.TransactionPlatform.Domain.Enums;

namespace Enterprise.TransactionPlatform.Application.Transactions.Submit
{
    public sealed record SubmitTransactionResult(Guid TransactionId, string Reference, decimal Amount, string Currency, TransactionType Type, TransactionStatus Status, string? Description, DateTime CreatedAtUtc);
}
