using Dapper;
using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Common;
using Enterprise.TransactionPlatform.Application.Transactions.Search;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Infrastructure.Persistence.Abstractions;
using Enterprise.TransactionPlatform.Infrastructure.Persistence.Mappers;
using Enterprise.TransactionPlatform.Infrastructure.Persistence.Models;
using Microsoft.Data.SqlClient;
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
    public async Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "dbo.sp_GetTransactionById",
            parameters: new
            {
                TransactionId = transactionId
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var record =
            await connection.QuerySingleOrDefaultAsync<TransactionRecord>(command);

        return record is null
            ? null
            : TransactionPersistenceMapper.ToDomain(record);
    }
    public async Task<Transaction?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "dbo.sp_GetTransactionByReference",
            parameters: new
            {
                Reference = reference
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var record = await connection.QuerySingleOrDefaultAsync<TransactionRecord>(command);

        return record is null
            ? null
            : TransactionPersistenceMapper.ToDomain(record);
    }
    public async Task UpdateStatusAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var record = TransactionPersistenceMapper.ToRecord(transaction);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "dbo.sp_UpdateTransactionStatus",
            parameters: new
            {
                record.TransactionId,
                record.Status,
                record.UpdatedAtUtc
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
    public async Task<PagedResult<Transaction>> SearchAsync(TransactionSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "dbo.sp_SearchTransactions",
            parameters: new
            {
                criteria.Reference,
                Status = criteria.Status?.ToString(),
                Type = criteria.Type?.ToString(),
                criteria.Currency,
                criteria.FromDateUtc,
                criteria.ToDateUtc,
                criteria.PageNumber,
                criteria.PageSize
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var totalRecords = await multi.ReadSingleAsync<int>();
        var records = (await multi.ReadAsync<TransactionRecord>())
            .ToArray();

        var transactions = records
            .Select(TransactionPersistenceMapper.ToDomain)
            .ToArray();

        return new PagedResult<Transaction>(transactions, totalRecords);
    }
}