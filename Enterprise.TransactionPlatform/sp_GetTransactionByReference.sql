CREATE OR ALTER PROCEDURE dbo.sp_GetTransactionByReference
    @Reference NVARCHAR(100)
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
    WHERE Reference = @Reference;
END;
GO