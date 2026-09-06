namespace Enterprise.TransactionPlatform.Application.Transactions.GetById
{
    public sealed record GetTransactionByIdResult(Guid TransactionId, string Reference, decimal Amount, string Currency, string Type, string Status, string? Description, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
}
