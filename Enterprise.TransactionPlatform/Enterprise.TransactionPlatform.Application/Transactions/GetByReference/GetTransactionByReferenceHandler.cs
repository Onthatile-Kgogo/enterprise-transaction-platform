using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Transactions.GetById;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.TransactionPlatform.Application.Transactions.GetByReference
{
    public sealed class GetTransactionByReferenceHandler
    {
        private readonly ITransactionRepository transactionRepository;

        public GetTransactionByReferenceHandler(ITransactionRepository transactionRepository)
        {
            this.transactionRepository = transactionRepository;
        }

        public async Task<GetTransactionByIdResult?> HandleAsync(GetTransactionByReferenceQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var transaction = await transactionRepository.GetByReferenceAsync(query.Reference, cancellationToken);

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
