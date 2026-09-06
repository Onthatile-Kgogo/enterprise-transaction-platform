using Enterprise.TransactionPlatform.Domain.Enums;

namespace Enterprise.TransactionPlatform.Application.Transactions.Submit
{
    public sealed record SubmitTransactionCommand(string Reference, decimal Amount, string Currency, TransactionType Type, string? Description);
}
