using Dapper;
using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Infrastructure.Persistence.Abstractions;
using Enterprise.TransactionPlatform.Infrastructure.Persistence.Mappers;
using System.Data;

namespace Enterprise.TransactionPlatform.Infrastructure.Persistence.Repositories;

internal sealed class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var record = TransactionPersistenceMapper.ToRecord(transaction);

        await using var connection = _connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "dbo.sp_CreateTransaction",
            parameters: new
            {
                record.TransactionId,
                record.Reference,
                record.Amount,
                record.Currency,
                record.Type,
                record.Status,
                record.Description,
                record.CreatedAtUtc,
                record.UpdatedAtUtc
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}