namespace Enterprise.TransactionPlatform.Application.Transactions.Search
{
    public sealed record SearchTransactionsQuery(string? Reference, string? Status, string? Type, string? Currency, DateTime? FromDateUtc, DateTime? ToDateUtc, int PageNumber = 1, int PageSize = 20);
}
