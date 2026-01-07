-- ============================================
-- Target Markets Module - Database Schema
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TargetMarkets')
BEGIN
    CREATE TABLE TargetMarkets (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        ImageUrl NVARCHAR(500) NULL,
        SubItemsJson NVARCHAR(MAX) NULL, -- Stores array of strings
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    CREATE INDEX IX_TargetMarkets_Name ON TargetMarkets(Name);
END
GO

-- 1. Create Target Market
CREATE OR ALTER PROCEDURE sp_CreateTargetMarket
    @Name NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @ImageUrl NVARCHAR(500),
    @SubItemsJson NVARCHAR(MAX),
    @NewId INT OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    INSERT INTO TargetMarkets (Name, Description, ImageUrl, SubItemsJson)
    VALUES (@Name, @Description, @ImageUrl, @SubItemsJson);
    SET @NewId = SCOPE_IDENTITY();
END
GO

-- 2. Update Target Market
CREATE OR ALTER PROCEDURE sp_UpdateTargetMarket
    @Id INT,
    @Name NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @ImageUrl NVARCHAR(500),
    @SubItemsJson NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE TargetMarkets 
    SET Name = @Name,
        Description = @Description,
        ImageUrl = ISNULL(@ImageUrl, ImageUrl),
        SubItemsJson = @SubItemsJson,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END
GO

-- 3. Get Target Market By Id
CREATE OR ALTER PROCEDURE sp_GetTargetMarketById
    @Id INT
AS
BEGIN
    SELECT * FROM TargetMarkets WHERE Id = @Id;
END
GO

-- 4. Get All Target Markets with Pagination and Search
CREATE OR ALTER PROCEDURE sp_GetTargetMarkets
    @Search NVARCHAR(200) = NULL,
    @Offset INT = 0,
    @Limit INT = 10
AS
BEGIN
    SELECT * FROM TargetMarkets
    WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%' OR Description LIKE '%' + @Search + '%')
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;

    SELECT COUNT(*) FROM TargetMarkets
    WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%' OR Description LIKE '%' + @Search + '%');
END
GO

-- 5. Delete Target Market
CREATE OR ALTER PROCEDURE sp_DeleteTargetMarket
    @Id INT
AS
BEGIN
    DELETE FROM TargetMarkets WHERE Id = @Id;
END
GO
