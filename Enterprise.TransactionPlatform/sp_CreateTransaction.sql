CREATE OR ALTER PROCEDURE dbo.sp_CreateTransaction
    @TransactionId UNIQUEIDENTIFIER,
    @Reference NVARCHAR(100),
    @Amount DECIMAL(18,2),
    @Currency NVARCHAR(10),
    @Type NVARCHAR(50),
    @Status NVARCHAR(50),
    @Description NVARCHAR(500) = NULL,
    @CreatedAtUtc DATETIME2(7),
    @UpdatedAtUtc DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Transactions
        WHERE Reference = @Reference
    )
    BEGIN
        THROW 50001, 'A transaction with this reference already exists.', 1;
    END;

    INSERT INTO dbo.Transactions
    (
        TransactionId,
        Reference,
        Amount,
        Currency,
        Type,
        Status,
        Description,
        CreatedAtUtc,
        UpdatedAtUtc
    )
    VALUES
    (
        @TransactionId,
        @Reference,
        @Amount,
        @Currency,
        @Type,
        @Status,
        @Description,
        @CreatedAtUtc,
        @UpdatedAtUtc
    );

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