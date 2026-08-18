CREATE OR ALTER PROCEDURE dbo.sp_UpdateTransactionStatus
    @TransactionId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @UpdatedAtUtc DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Transactions
    SET
        Status = @Status,
        UpdatedAtUtc = @UpdatedAtUtc
    WHERE TransactionId = @TransactionId;
END;
GO