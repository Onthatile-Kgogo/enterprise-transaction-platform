namespace Enterprise.TransactionPlatform.Application.Transactions.Search
{
    public sealed record TransactionSearchItem(Guid TransactionId, string Reference, decimal Amount, string Currency, string Type, string Status, string? Description, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
}
