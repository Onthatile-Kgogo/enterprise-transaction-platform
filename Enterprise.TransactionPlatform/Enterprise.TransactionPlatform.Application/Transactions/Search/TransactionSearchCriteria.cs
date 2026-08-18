using Enterprise.TransactionPlatform.Domain.Enums;

namespace Enterprise.TransactionPlatform.Application.Transactions.Search
{
    public sealed record TransactionSearchCriteria(string? Reference, TransactionStatus? Status, TransactionType? Type, string? Currency, DateTime? FromDateUtc, DateTime? ToDateUtc, int PageNumber, int PageSize);
}
