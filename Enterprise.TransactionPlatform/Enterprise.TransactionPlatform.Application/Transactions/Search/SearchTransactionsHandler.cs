using Enterprise.TransactionPlatform.Application.Abstractions.Currencies;
using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Common.Results;
using Enterprise.TransactionPlatform.Application.Transactions.Search;
using Enterprise.TransactionPlatform.Domain.Enums;

public sealed class SearchTransactionsHandler
{
    private readonly ITransactionRepository transactionRepository;
    private readonly ISupportedCurrencyProvider supportedCurrencyProvider;

    public SearchTransactionsHandler(ITransactionRepository transactionRepository, ISupportedCurrencyProvider supportedCurrencyProvider)
    {
        ArgumentNullException.ThrowIfNull(transactionRepository);
        ArgumentNullException.ThrowIfNull(supportedCurrencyProvider);

        this.transactionRepository = transactionRepository;
        this.supportedCurrencyProvider = supportedCurrencyProvider;
    }

    public async Task<ApplicationResult<SearchTransactionsResult>> HandleAsync(SearchTransactionsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var validationError = SearchTransactionsValidator.Validate(query);
        if (validationError is not null)
        {
            return ApplicationResult<SearchTransactionsResult>.Failure("transaction_search.validation_failed", validationError);
        }

        if (!string.IsNullOrWhiteSpace(query.Currency))
        {
            var isSupported = await supportedCurrencyProvider.IsSupportedAsync(query.Currency, cancellationToken);
            if (!isSupported)
            {
                return ApplicationResult<SearchTransactionsResult>.Failure("transaction_search.unsupported_currency", $"Unsupported currency '{query.Currency}'.");
            }
        }

        TransactionStatus? status = null;

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            status = Enum.Parse<TransactionStatus>(query.Status, ignoreCase: true);
        }

        TransactionType? type = null;

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            type = Enum.Parse<TransactionType>(query.Type, ignoreCase: true);
        }

        var criteria = new TransactionSearchCriteria(
            Reference: query.Reference?.Trim(),
            Status: status,
            Type: type,
            Currency: query.Currency?.Trim().ToUpperInvariant(),
            FromDateUtc: query.FromDateUtc,
            ToDateUtc: query.ToDateUtc,
            PageNumber: query.PageNumber,
            PageSize: query.PageSize);

        var pagedResult = await transactionRepository.SearchAsync(criteria, cancellationToken);

        var items = pagedResult.Items
            .Select(transaction => new TransactionSearchItem(
                transaction.TransactionId,
                transaction.Reference.Value,
                transaction.Money.Amount,
                transaction.Money.Currency.Code,
                transaction.Type.ToString(),
                transaction.Status.ToString(),
                transaction.Description,
                transaction.CreatedAtUtc,
                transaction.UpdatedAtUtc))
            .ToArray();

        var totalPages = pagedResult.TotalRecords == 0
            ? 0
            : (int)Math.Ceiling(pagedResult.TotalRecords / (double)query.PageSize);

        var result = new SearchTransactionsResult(
            Items: items,
            PageNumber: query.PageNumber,
            PageSize: query.PageSize,
            TotalRecords: pagedResult.TotalRecords,
            TotalPages: totalPages);

        return ApplicationResult<SearchTransactionsResult>.Success(result);
    }
}