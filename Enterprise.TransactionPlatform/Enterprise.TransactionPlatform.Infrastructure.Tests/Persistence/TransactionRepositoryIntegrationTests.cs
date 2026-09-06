using Dapper;
using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Transactions.Search;
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

            var transaction = Transaction.Create(
                reference,
                money,
                TransactionType.Payment,
                "Infrastructure integration test");

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

            var transaction = Transaction.Create(
                reference,
                money,
                TransactionType.Payment,
                "Query test transaction");

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

        [Fact]
        public async Task UpdateStatusAsync_WhenStatusChanges_ShouldPersistUpdatedStatus()
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

            var reference = TransactionReference.Create($"TXN-STATUS-{Guid.NewGuid():N}");
            var currency = Currency.Create("ZAR");
            var money = Money.Create(100m, currency);
            var transaction = Transaction.Create(reference, money, TransactionType.Payment, "Infrastructure status update test");

            try
            {
                await repository.AddAsync(transaction, CancellationToken.None);

                transaction.MarkPending();

                var expectedUpdatedAtUtc = transaction.UpdatedAtUtc;

                // Act
                await repository.UpdateStatusAsync(transaction, CancellationToken.None);

                var persistedTransaction = await repository.GetByIdAsync(transaction.TransactionId, CancellationToken.None);

                // Assert
                Assert.NotNull(persistedTransaction);
                Assert.Equal(transaction.TransactionId, persistedTransaction.TransactionId);
                Assert.Equal(TransactionStatus.Pending, persistedTransaction.Status);
                Assert.NotNull(expectedUpdatedAtUtc);
                Assert.NotNull(persistedTransaction.UpdatedAtUtc);
                var difference = Math.Abs((persistedTransaction.UpdatedAtUtc.Value - expectedUpdatedAtUtc.Value).TotalMilliseconds);
                Assert.True(difference < 5, $"Expected UpdatedAtUtc to be within 5ms. Difference was {difference}ms.");
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

                await cleanupConnection.ExecuteAsync(
                    deleteSql,
                    new
                    {
                        transaction.TransactionId
                    });
            }
        }

        [Fact]
        public async Task SearchAsync_WithNoFilters_ShouldReturnPagedTransactions()
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

            var repository =
                serviceProvider.GetRequiredService<ITransactionRepository>();

            var criteria = new TransactionSearchCriteria(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null,
                PageNumber: 1,
                PageSize: 10);

            // Act
            var result = await repository.SearchAsync(criteria);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalRecords >= result.Items.Count);
            Assert.True(result.Items.Count <= 10);
        }

        [Fact]
        public async Task SearchAsync_WithReference_ShouldReturnMatchingTransaction()
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

            var repository =
                serviceProvider.GetRequiredService<ITransactionRepository>();

            var referenceValue = $"SEARCH-{Guid.NewGuid():N}";

            var transaction = Transaction.Create(TransactionReference.Create(referenceValue),
                Money.Create(
                    250m,
                    Currency.Create("ZAR")),
                TransactionType.Payment,
                "Search integration test");

            try
            {
                await repository.AddAsync(transaction);

                var criteria = new TransactionSearchCriteria(
                    Reference: referenceValue,
                    Status: null,
                    Type: null,
                    Currency: null,
                    FromDateUtc: null,
                    ToDateUtc: null,
                    PageNumber: 1,
                    PageSize: 10);

                // Act
                var result = await repository.SearchAsync(criteria);

                // Assert
                Assert.Equal(1, result.TotalRecords);

                var found = Assert.Single(result.Items);

                Assert.Equal(
                    transaction.TransactionId,
                    found.TransactionId);

                Assert.Equal(
                    referenceValue,
                    found.Reference.Value);
            }
            finally
            {
                await using var cleanupConnection =
                    new SqlConnection(ConnectionString);

                await cleanupConnection.OpenAsync();

                const string deleteSql =
                """
                    DELETE FROM dbo.Transactions
                    WHERE TransactionId = @TransactionId;
                """;

                await cleanupConnection.ExecuteAsync(
                    deleteSql,
                    new
                    {
                        transaction.TransactionId
                    });
            }
        }

        [Fact]
        public async Task SearchAsync_WithPageSize_ShouldRespectPagination()
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

            var repository =
                serviceProvider.GetRequiredService<ITransactionRepository>();

            var criteria = new TransactionSearchCriteria(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null,
                PageNumber: 1,
                PageSize: 2);

            // Act
            var result = await repository.SearchAsync(criteria);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Items.Count <= 2);
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