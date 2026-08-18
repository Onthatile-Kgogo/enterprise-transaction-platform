using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;

namespace Enterprise.TransactionPlatform.Application.Transactions.UpdateStatus
{
    public sealed class UpdateTransactionStatusHandler
    {
        private readonly ITransactionRepository transactionRepository;

        public UpdateTransactionStatusHandler(ITransactionRepository transactionRepository)
        {
            ArgumentNullException.ThrowIfNull(transactionRepository);

            this.transactionRepository = transactionRepository;
        }

        public async Task<UpdateTransactionStatusResult> HandleAsync(UpdateTransactionStatusCommand command, CancellationToken cancellationToken)
        {
            var transaction = await transactionRepository.GetByIdAsync(command.TransactionId, cancellationToken);

            if (transaction is null)
                throw new KeyNotFoundException($"Transaction '{command.TransactionId}' was not found.");


            switch (command.Status)
            {
                case TransactionStatus.Pending:
                    transaction.MarkPending();
                    break;

                case TransactionStatus.Processing:
                    transaction.StartProcessing();
                    break;

                case TransactionStatus.Completed:
                    transaction.Complete();
                    break;

                case TransactionStatus.Failed:
                    transaction.Fail();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(command.Status),
                        command.Status,
                        "Transaction status is invalid.");
            }

            await transactionRepository.UpdateStatusAsync(
                transaction,
                cancellationToken);

            return new UpdateTransactionStatusResult(
                transaction.TransactionId,
                transaction.Reference.Value,
                transaction.Status,
                transaction.UpdatedAtUtc);
        }
    }
}
