using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Common.Results;
using Enterprise.TransactionPlatform.Application.Currencies;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Application.Transactions.Submit
{

    public sealed class SubmitTransactionHandler
    {
        private readonly CurrencyValidator _currencyValidator;
        private readonly ITransactionRepository _transactionRepository;

        public SubmitTransactionHandler(CurrencyValidator currencyValidator, ITransactionRepository transactionRepository)
        {
            ArgumentNullException.ThrowIfNull(currencyValidator);
            ArgumentNullException.ThrowIfNull(transactionRepository);

            _currencyValidator = currencyValidator;
            _transactionRepository = transactionRepository;
        }

        public async Task<ApplicationResult<SubmitTransactionResult>> HandleAsync(SubmitTransactionCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            TransactionReference reference;
            Currency currency;
            Money money;
            Transaction transaction;

            try
            {
                reference = TransactionReference.Create(command.Reference);
            }
            catch (ArgumentException exception)
            {
                return ApplicationResult<SubmitTransactionResult>.Failure(
                    "TRANSACTION.INVALID_REFERENCE",
                    exception.Message);
            }

            try
            {
                currency = Currency.Create(command.Currency);
            }
            catch (ArgumentException exception)
            {
                return ApplicationResult<SubmitTransactionResult>.Failure(
                    "CURRENCY.INVALID_FORMAT",
                    exception.Message);
            }

            var currencyValidation = await _currencyValidator.ValidateAsync(currency, cancellationToken);
            if (!currencyValidation.IsValid)
            {
                return ApplicationResult<SubmitTransactionResult>.Failure(
                    "CURRENCY.UNSUPPORTED",
                    currencyValidation.Error ?? "Currency is not supported.");
            }

            try
            {
                money = Money.Create(command.Amount, currency);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return ApplicationResult<SubmitTransactionResult>.Failure(
                    "TRANSACTION.INVALID_AMOUNT",
                    exception.Message);
            }

            try
            {
                transaction = Transaction.Create(reference, money, command.Type, command.Description);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return ApplicationResult<SubmitTransactionResult>.Failure(
                    "TRANSACTION.INVALID_TYPE",
                    exception.Message);
            }
            catch (ArgumentException exception)
            {
                return ApplicationResult<SubmitTransactionResult>.Failure(
                    "TRANSACTION.INVALID_REQUEST",
                    exception.Message);
            }

            await _transactionRepository.AddAsync(transaction, cancellationToken);

            var result = new SubmitTransactionResult(
                transaction.TransactionId,
                transaction.Reference.Value,
                transaction.Money.Amount,
                transaction.Money.Currency.Code,
                transaction.Type,
                transaction.Status,
                transaction.Description,
                transaction.CreatedAtUtc);

            return ApplicationResult<SubmitTransactionResult>.Success(result);
        }
    }
}
