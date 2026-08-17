using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;

namespace Enterprise.TransactionPlatform.Application.Transactions.GetById
{
    public sealed class GetTransactionByIdHandler
    {
        private readonly ITransactionRepository transactionRepository;

        public GetTransactionByIdHandler(ITransactionRepository transactionRepository)
        {
            this.transactionRepository = transactionRepository;
        }

        public async Task<GetTransactionByIdResult?> HandleAsync(GetTransactionByIdQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var transaction = await transactionRepository.GetByIdAsync(query.TransactionId, cancellationToken);

            if (transaction is null)
                return null;


            return new GetTransactionByIdResult(
                TransactionId: transaction.TransactionId,
                Reference: transaction.Reference.Value,
                Amount: transaction.Money.Amount,
                Currency: transaction.Money.Currency.Code,
                Type: transaction.Type.ToString(),
                Status: transaction.Status.ToString(),
                Description: transaction.Description,
                CreatedAtUtc: transaction.CreatedAtUtc,
                UpdatedAtUtc: transaction.UpdatedAtUtc);
        }
    }
}
