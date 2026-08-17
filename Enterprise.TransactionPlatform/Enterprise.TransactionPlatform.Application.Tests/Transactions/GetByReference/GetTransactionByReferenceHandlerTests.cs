using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Transactions.GetByReference;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Application.Tests.Transactions.GetByReference
{
    public class GetTransactionByReferenceHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WhenTransactionExists_ShouldReturnResult()
        {
            var transaction = Transaction.Create(
                TransactionReference.Create("TXN-GET-REF-001"),
                Money.Create(500m, Currency.Create("ZAR")),
                TransactionType.Payment,
                "Get by reference test");

            var repository = new TestTransactionRepository(transaction);
            var handler = new GetTransactionByReferenceHandler(repository);
            var query = new GetTransactionByReferenceQuery(transaction.Reference.Value);

            var result = await handler.HandleAsync(query);

            Assert.NotNull(result);
            Assert.Equal(transaction.TransactionId, result.TransactionId);
            Assert.Equal(transaction.Reference.Value, result.Reference);
        }

        [Fact]
        public async Task HandleAsync_WhenTransactionDoesNotExist_ShouldReturnNull()
        {
            var repository = new TestTransactionRepository();
            var handler = new GetTransactionByReferenceHandler(repository);
            var query = new GetTransactionByReferenceQuery($"MISSING-{Guid.NewGuid():N}");
            var result = await handler.HandleAsync(query);

            Assert.Null(result);
        }

        private sealed class TestTransactionRepository : ITransactionRepository
        {
            private readonly Transaction? _transaction;
            public TestTransactionRepository(Transaction? transaction = null)
            {
                _transaction = transaction;
            }

            public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
            {
                var result = _transaction?.TransactionId == transactionId
                        ? _transaction
                        : null;

                return Task.FromResult(result);
            }

            public Task<Transaction?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
            {
                var result = _transaction?.Reference.Value == reference
                        ? _transaction
                        : null;

                return Task.FromResult(result);
            }
        }
    }
}
