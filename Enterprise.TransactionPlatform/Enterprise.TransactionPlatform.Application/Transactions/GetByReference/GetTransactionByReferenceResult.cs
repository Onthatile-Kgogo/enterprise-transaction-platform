namespace Enterprise.TransactionPlatform.Application.Transactions.GetByReference
{
    public sealed record GetTransactionByReferenceResult(Guid TransactionId, string Reference, decimal Amount, string Currency, string Type, string Status, string? Description, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
}