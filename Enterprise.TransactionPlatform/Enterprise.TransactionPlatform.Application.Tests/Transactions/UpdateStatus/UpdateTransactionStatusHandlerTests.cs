using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Transactions.UpdateStatus;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Domain.Exceptions;
using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Application.Tests.Transactions.UpdateStatus
{
    public class UpdateTransactionStatusHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WhenStatusTransitionIsValid_ShouldUpdateTransaction()
        {
            var transaction = CreateTransaction();
            var repository = new TestTransactionRepository(transaction);
            var handler = new UpdateTransactionStatusHandler(repository);

            var command = new UpdateTransactionStatusCommand(transaction.TransactionId, TransactionStatus.Pending);
            var result = await handler.HandleAsync(command, CancellationToken.None);

            Assert.Equal(TransactionStatus.Pending, result.Status);
            Assert.True(repository.UpdateCalled);
        }

        [Fact]
        public async Task HandleAsync_WhenTransactionDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            var repository = new TestTransactionRepository(null);
            var handler = new UpdateTransactionStatusHandler(repository);
            var command = new UpdateTransactionStatusCommand(Guid.NewGuid(), TransactionStatus.Pending);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.HandleAsync(command, CancellationToken.None));
        }

        [Fact]
        public async Task HandleAsync_WhenTransitionIsInvalid_ShouldNotPersistUpdate()
        {
            var transaction = CreateTransaction();
            var repository = new TestTransactionRepository(transaction);
            var handler = new UpdateTransactionStatusHandler(repository);

            var command = new UpdateTransactionStatusCommand(transaction.TransactionId, TransactionStatus.Completed);

            await Assert.ThrowsAsync<DomainException>(() =>
                handler.HandleAsync(command, CancellationToken.None));

            Assert.False(repository.UpdateCalled);
        }

        private static Transaction CreateTransaction()
        {
            var reference =
                TransactionReference.Create("TXN-STATUS-HANDLER-001");

            var currency =
                Currency.Create("ZAR");

            var money =
                Money.Create(100m, currency);

            return Transaction.Create(reference, money, TransactionType.Payment, "Transaction status handler test");
        }

        private sealed class TestTransactionRepository : ITransactionRepository
        {
            private readonly Transaction? transaction;

            public bool UpdateCalled { get; private set; }

            public TestTransactionRepository(Transaction? transaction)
            {
                this.transaction = transaction;
            }

            public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(transaction);
            }

            public Task<Transaction?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<Transaction?>(null);
            }

            public Task UpdateStatusAsync(Transaction transaction, CancellationToken cancellationToken = default)
            {
                UpdateCalled = true;
                return Task.CompletedTask;
            }
        }
    }
}
