using Enterprise.TransactionPlatform.Application.Abstractions.Currencies;
using Enterprise.TransactionPlatform.Application.Currencies;
using Enterprise.TransactionPlatform.Application.Transactions.Submit;
using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Domain.Entities;

namespace Enterprise.TransactionPlatform.Application.Tests.Transactions.Submit
{
    public class SubmitTransactionHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessfulResult()
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR");
            var validator = new CurrencyValidator(provider);
            var repository = new TestTransactionRepository();

            var handler = new SubmitTransactionHandler(validator, repository);

            var command = new SubmitTransactionCommand("TXN-001", 1500.00m, "zar", TransactionType.Payment, "Test payment");

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value.TransactionId);
            Assert.Equal("TXN-001", result.Value.Reference);
            Assert.Equal(1500.00m, result.Value.Amount);
            Assert.Equal("ZAR", result.Value.Currency);
            Assert.Equal(TransactionType.Payment, result.Value.Type);
            Assert.Equal(TransactionStatus.Received, result.Value.Status);
            Assert.Equal("Test payment", result.Value.Description);
            Assert.NotEqual(default, result.Value.CreatedAtUtc);
            Assert.NotNull(repository.SavedTransaction);
            Assert.Equal(result.Value.TransactionId, repository.SavedTransaction.TransactionId);
        }

        [Fact]
        public async Task HandleAsync_WithUnsupportedCurrency_ShouldReturnFailure()
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR");
            var validator = new CurrencyValidator(provider);
            var repository = new TestTransactionRepository();

            var handler = new SubmitTransactionHandler(validator, repository);
            var command = new SubmitTransactionCommand("TXN-001", 100m, "USD", TransactionType.Payment, null);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.NotNull(result.Error);
            Assert.Equal("CURRENCY.UNSUPPORTED", result.Error.Code);
            Assert.Null(repository.SavedTransaction);
        }

        [Fact]
        public async Task HandleAsync_WithInvalidCurrencyFormat_ShouldReturnFailure()
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR");
            var validator = new CurrencyValidator(provider);
            var repository = new TestTransactionRepository();

            var handler = new SubmitTransactionHandler(validator, repository);
            var command = new SubmitTransactionCommand("TXN-001", 100m, "12A", TransactionType.Payment, null);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("CURRENCY.INVALID_FORMAT", result.Error?.Code);
            Assert.Null(repository.SavedTransaction);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task HandleAsync_WithInvalidAmount_ShouldReturnFailure(decimal amount)
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR");
            var validator = new CurrencyValidator(provider);
            var repository = new TestTransactionRepository();

            var handler = new SubmitTransactionHandler(validator, repository);
            var command = new SubmitTransactionCommand("TXN-001", amount, "ZAR", TransactionType.Payment, null);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("TRANSACTION.INVALID_AMOUNT", result.Error?.Code);
            Assert.Null(repository.SavedTransaction);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task HandleAsync_WithInvalidReference_ShouldReturnFailure(string reference)
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR");
            var validator = new CurrencyValidator(provider);
            var repository = new TestTransactionRepository();

            var handler = new SubmitTransactionHandler(validator, repository);
            var command = new SubmitTransactionCommand(reference, 100m, "ZAR", TransactionType.Payment, null);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            Assert.False(result.IsSuccess);

            Assert.Equal(
                "TRANSACTION.INVALID_REFERENCE",
                result.Error?.Code);

            Assert.Null(repository.SavedTransaction);
        }

        [Fact]
        public async Task HandleAsync_WithInvalidTransactionType_ShouldReturnFailure()
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR");
            var validator = new CurrencyValidator(provider);
            var repository = new TestTransactionRepository();

            var handler = new SubmitTransactionHandler(validator, repository);
            var command = new SubmitTransactionCommand("TXN-001", 100m, "ZAR", (TransactionType)999, null);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("TRANSACTION.INVALID_TYPE", result.Error?.Code);
            Assert.Null(repository.SavedTransaction);
        }

        [Fact]
        public async Task HandleAsync_WithDescriptionLongerThan500Characters_ShouldReturnFailure()
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR");
            var validator = new CurrencyValidator(provider);
            var repository = new TestTransactionRepository();

            var handler = new SubmitTransactionHandler(validator, repository);
            var command = new SubmitTransactionCommand("TXN-001", 100m, "ZAR", TransactionType.Payment, new string('A', 501));

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("TRANSACTION.INVALID_REQUEST", result.Error?.Code);
            Assert.Null(repository.SavedTransaction);
        }

        [Fact]
        public async Task HandleAsync_WithNullCommand_ShouldThrowArgumentNullException()
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR");
            var validator = new CurrencyValidator(provider);
            var repository = new TestTransactionRepository();

            var handler = new SubmitTransactionHandler(validator, repository);

            // Act
            var action = async () => await handler.HandleAsync(null!);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
            Assert.Null(repository.SavedTransaction);
        }

        private sealed class TestSupportedCurrencyProvider : ISupportedCurrencyProvider
        {
            private readonly HashSet<string> _supportedCurrencies;

            public TestSupportedCurrencyProvider(params string[] supportedCurrencies)
            {
                _supportedCurrencies = supportedCurrencies
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            public Task<bool> IsSupportedAsync(string currencyCode, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_supportedCurrencies.Contains(currencyCode));
            }
        }

        private sealed class TestTransactionRepository : ITransactionRepository
        {
            public Transaction? SavedTransaction { get; private set; }

            public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
            {
                SavedTransaction = transaction;
                return Task.CompletedTask;
            }
            public Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<Transaction?>(null);
            }
            public Task<Transaction?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<Transaction?>(null);
            }
        }
    }
}
