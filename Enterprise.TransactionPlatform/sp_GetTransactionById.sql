CREATE OR ALTER PROCEDURE dbo.sp_GetTransactionById
    @TransactionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

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
END;
GO