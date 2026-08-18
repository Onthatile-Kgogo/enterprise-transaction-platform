using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Common;
using Enterprise.TransactionPlatform.Application.Transactions.GetById;
using Enterprise.TransactionPlatform.Application.Transactions.Search;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Application.Tests.Transactions.GetById
{
    public class GetTransactionByIdHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WhenTransactionExists_ShouldReturnResult()
        {
            var transaction = Transaction.Create(
                TransactionReference.Create("TXN-GET-ID-001"),
                Money.Create(250m, Currency.Create("ZAR")),
                TransactionType.Payment,
                "Get by id test");

            var repository = new TestTransactionRepository(transaction);
            var handler = new GetTransactionByIdHandler(repository);
            var query = new GetTransactionByIdQuery(transaction.TransactionId);
            var result = await handler.HandleAsync(query);

            Assert.NotNull(result);
            Assert.Equal(transaction.TransactionId, result.TransactionId);
            Assert.Equal(transaction.Reference.Value, result.Reference);
            Assert.Equal(transaction.Money.Amount, result.Amount);
            Assert.Equal(transaction.Money.Currency.Code, result.Currency);
            Assert.Equal(transaction.Type.ToString(), result.Type);
            Assert.Equal(transaction.Status.ToString(), result.Status);
        }

        [Fact]
        public async Task HandleAsync_WhenTransactionDoesNotExist_ShouldReturnNull()
        {
            var repository = new TestTransactionRepository();
            var handler = new GetTransactionByIdHandler(repository);
            var query = new GetTransactionByIdQuery(Guid.NewGuid());
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

            public Task<PagedResult<Transaction>> SearchAsync(TransactionSearchCriteria criteria, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new PagedResult<Transaction>(Array.Empty<Transaction>(), 0));
            }

            public Task UpdateStatusAsync(Transaction transaction, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
