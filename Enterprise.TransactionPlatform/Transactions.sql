USE EnterpriseTransactionPlatform;
GO

IF OBJECT_ID('dbo.Transactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transactions
    (
        TransactionId UNIQUEIDENTIFIER NOT NULL,
        Reference NVARCHAR(100) NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        Currency NVARCHAR(10) NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        Description NVARCHAR(500) NULL,
        CreatedAtUtc DATETIME2(7) NOT NULL,
        UpdatedAtUtc DATETIME2(7) NULL,

        CONSTRAINT PK_Transactions
            PRIMARY KEY (TransactionId),

        CONSTRAINT UQ_Transactions_Reference
            UNIQUE (Reference)
    );
END;
GO