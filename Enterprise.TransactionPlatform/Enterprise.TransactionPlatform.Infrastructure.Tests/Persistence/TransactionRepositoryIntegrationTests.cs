using Dapper;
using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Transactions.GetById;
using Enterprise.TransactionPlatform.Application.Transactions.GetByReference;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Domain.ValueObjects;
using Enterprise.TransactionPlatform.Infrastructure.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.TransactionPlatform.Infrastructure.Tests.Persistence
{
    public sealed class TransactionRepositoryIntegrationTests
    {
        private const string ConnectionString = "Server=Onthatile_PC;Database=EnterpriseTransactionPlatform;Trusted_Connection=True;TrustServerCertificate=True;";

        [Fact]
        public async Task AddAsync_WithValidTransaction_ShouldPersistTransaction()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:EnterpriseTransactionPlatform"] =
                        ConnectionString
                })
                .Build();

            var services = new ServiceCollection();
            services.AddInfrastructure(configuration);
            await using var serviceProvider = services.BuildServiceProvider();

            var repository = serviceProvider.GetRequiredService<ITransactionRepository>();
            var referenceValue = $"TEST-{Guid.NewGuid():N}";
            var reference = TransactionReference.Create(referenceValue);
            var currency = Currency.Create("ZAR");
            var money = Money.Create(1500.00m, currency);

            var transaction = Transaction.Create(reference, money, TransactionType.Payment, "Infrastructure integration test");

            try
            {
                // Act
                await repository.AddAsync(transaction);

                // Assert
                await using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();

                const string selectSql = """
                    SELECT
                        TransactionId,
                        Reference,
                        Amount,
                        Currency,
                        Type,
                        Status,
                        Description,
                        CreatedAtUtc,
                        UpdatedAtUtc
                    FROM dbo.Transactions
                    WHERE TransactionId = @TransactionId;
                    """;

                var persisted =
                    await connection.QuerySingleOrDefaultAsync<TransactionRow>(
                        selectSql,
                        new
                        {
                            transaction.TransactionId
                        });

                Assert.NotNull(persisted);
                Assert.Equal(transaction.TransactionId, persisted.TransactionId);
                Assert.Equal(transaction.Reference.Value, persisted.Reference);
                Assert.Equal(transaction.Money.Amount, persisted.Amount);
                Assert.Equal(transaction.Money.Currency.Code, persisted.Currency);
                Assert.Equal(transaction.Type.ToString(), persisted.Type);
                Assert.Equal(transaction.Status.ToString(), persisted.Status);
                Assert.Equal(transaction.Description, persisted.Description);
            }
            finally
            {
                await using var cleanupConnection = new SqlConnection(ConnectionString);
                await cleanupConnection.OpenAsync();

                const string deleteSql =
                    """
                        DELETE FROM dbo.Transactions
                        WHERE TransactionId = @TransactionId;
                    """;

                await cleanupConnection.ExecuteAsync(deleteSql, new
                {
                    transaction.TransactionId
                });
            }
        }

        [Fact]
        public async Task GetByIdAsync_WhenTransactionExists_ShouldReturnTransaction()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                 .AddInMemoryCollection(new Dictionary<string, string?>
                 {
                     ["ConnectionStrings:EnterpriseTransactionPlatform"] = ConnectionString
                 })
                .Build();

            var services = new ServiceCollection();
            services.AddInfrastructure(configuration);

            await using var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetRequiredService<ITransactionRepository>();

            var referenceValue = $"TEST-{Guid.NewGuid():N}";
            var reference = TransactionReference.Create(referenceValue);
            var currency = Currency.Create("ZAR");
            var money = Money.Create(100m, currency);

            var transaction = Transaction.Create(reference, money, TransactionType.Payment, "Query test transaction");

            try
            {
                await repository.AddAsync(transaction);

                // Act
                var result = await repository.GetByIdAsync(transaction.TransactionId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(transaction.TransactionId, result.TransactionId);
                Assert.Equal(transaction.Reference.Value, result.Reference.Value);
                Assert.Equal(transaction.Money.Amount, result.Money.Amount);
                Assert.Equal(transaction.Money.Currency.Code, result.Money.Currency.Code);
                Assert.Equal(transaction.Type, result.Type);
                Assert.Equal(transaction.Status, result.Status);
            }
            finally
            {
                await using var cleanupConnection = new SqlConnection(ConnectionString);
                await cleanupConnection.OpenAsync();

                const string deleteSql =
                    """
                        DELETE FROM dbo.Transactions
                        WHERE TransactionId = @TransactionId;
                    """;

                await cleanupConnection.ExecuteAsync(deleteSql, new
                {
                    transaction.TransactionId
                });
            }
        }

        [Fact]
        public async Task GetByIdAsync_WhenTransactionDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:EnterpriseTransactionPlatform"] =
                        ConnectionString
                })
                .Build();

            var services = new ServiceCollection();
            services.AddInfrastructure(configuration);

            await using var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetRequiredService<ITransactionRepository>();

            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByReferenceAsync_WhenTransactionExists_ShouldReturnTransaction()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:EnterpriseTransactionPlatform"] =
                        ConnectionString
                })
                .Build();

            var services = new ServiceCollection();
            services.AddInfrastructure(configuration);

            await using var serviceProvider = services.BuildServiceProvider();

            var repository = serviceProvider.GetRequiredService<ITransactionRepository>();

            var referenceValue = $"TEST-{Guid.NewGuid():N}";
            var reference = TransactionReference.Create(referenceValue);
            var currency = Currency.Create("ZAR");
            var money = Money.Create(100m, currency);

            var transaction = Transaction.Create(reference, money, TransactionType.Payment, "Reference query test");

            try
            {
                await repository.AddAsync(transaction);

                // Act
                var result = await repository.GetByReferenceAsync(referenceValue);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(transaction.TransactionId, result.TransactionId);
                Assert.Equal(transaction.Reference.Value, result.Reference.Value);
                Assert.Equal(transaction.Money.Amount, result.Money.Amount);
                Assert.Equal(transaction.Money.Currency.Code, result.Money.Currency.Code);
                Assert.Equal(transaction.Type, result.Type);
                Assert.Equal(transaction.Status, result.Status);
            }
            finally
            {
                await using var cleanupConnection = new SqlConnection(ConnectionString);
                await cleanupConnection.OpenAsync();

                const string deleteSql =
                    """
                        DELETE FROM dbo.Transactions
                        WHERE TransactionId = @TransactionId;
                    """;

                await cleanupConnection.ExecuteAsync(deleteSql, new
                {
                    transaction.TransactionId
                });
            }
        }

        [Fact]
        public async Task GetByReferenceAsync_WhenTransactionDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:EnterpriseTransactionPlatform"] = ConnectionString
                })
                .Build();

            var services = new ServiceCollection();
            services.AddInfrastructure(configuration);

            await using var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetRequiredService<ITransactionRepository>();

            // Act
            var result = await repository.GetByReferenceAsync($"MISSING-{Guid.NewGuid():N}");

            // Assert
            Assert.Null(result);
        }

        private sealed class TransactionRow
        {
            public Guid TransactionId { get; init; }
            public string Reference { get; init; } = string.Empty;
            public decimal Amount { get; init; }
            public string Currency { get; init; } = string.Empty;
            public string Type { get; init; } = string.Empty;
            public string Status { get; init; } = string.Empty;
            public string? Description { get; init; }
            public DateTime CreatedAtUtc { get; init; }
            public DateTime? UpdatedAtUtc { get; init; }
        }
    }
}
