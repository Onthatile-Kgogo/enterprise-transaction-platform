namespace Enterprise.TransactionPlatform.Application.Transactions.Search
{
    public sealed record SearchTransactionsResult(IReadOnlyCollection<TransactionSearchItem> Items, int PageNumber, int PageSize, int TotalRecords, int TotalPages);
}
