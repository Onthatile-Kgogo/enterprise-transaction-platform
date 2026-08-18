CREATE OR ALTER PROCEDURE dbo.sp_SearchTransactions
    @Reference      NVARCHAR(100) = NULL,
    @Status         NVARCHAR(50) = NULL,
    @Type           NVARCHAR(50) = NULL,
    @Currency       NVARCHAR(10) = NULL,
    @FromDateUtc    DATETIME2 = NULL,
    @ToDateUtc      DATETIME2 = NULL,
    @PageNumber     INT = 1,
    @PageSize       INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT COUNT(1) AS TotalRecords
    FROM dbo.Transactions
    WHERE (@Reference IS NULL OR Reference LIKE '%' + @Reference + '%')
      AND (@Status IS NULL OR Status = @Status)
      AND (@Type IS NULL OR Type = @Type)
      AND (@Currency IS NULL OR Currency = @Currency)
      AND (@FromDateUtc IS NULL OR CreatedAtUtc >= @FromDateUtc)
      AND (@ToDateUtc IS NULL OR CreatedAtUtc <= @ToDateUtc);

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
    WHERE (@Reference IS NULL OR Reference LIKE '%' + @Reference + '%')
      AND (@Status IS NULL OR Status = @Status)
      AND (@Type IS NULL OR Type = @Type)
      AND (@Currency IS NULL OR Currency = @Currency)
      AND (@FromDateUtc IS NULL OR CreatedAtUtc >= @FromDateUtc)
      AND (@ToDateUtc IS NULL OR CreatedAtUtc <= @ToDateUtc)
    ORDER BY CreatedAtUtc DESC, TransactionId DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;