-- ============================================
-- Products Module - Database Schema
-- ============================================

-- 1. Master Tables
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryMaster')
BEGIN
    CREATE TABLE CategoryMaster (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL UNIQUE,
        Image NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubCategoryMaster')
BEGIN
    CREATE TABLE SubCategoryMaster (
        Id INT PRIMARY KEY IDENTITY(1,1),
        CategoryId INT NOT NULL FOREIGN KEY REFERENCES CategoryMaster(Id) ON DELETE CASCADE,
        Name NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BrandMaster')
BEGIN
    CREATE TABLE BrandMaster (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        Logo NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UnitMaster')
BEGIN
    CREATE TABLE UnitMaster (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(50) NOT NULL UNIQUE
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CountryMaster')
BEGIN
    CREATE TABLE CountryMaster (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL UNIQUE
    );
END

-- 2. Product Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(500) NOT NULL,
        ShortDescription NVARCHAR(MAX) NULL,
        MainImage NVARCHAR(500) NULL,
        
        -- Master Data IDs
        CategoryId INT NULL FOREIGN KEY REFERENCES CategoryMaster(Id),
        SubCategoryId INT NULL FOREIGN KEY REFERENCES SubCategoryMaster(Id),
        BrandId INT NULL FOREIGN KEY REFERENCES BrandMaster(Id),
        UnitId INT NULL FOREIGN KEY REFERENCES UnitMaster(Id),
        CountryId INT NULL FOREIGN KEY REFERENCES CountryMaster(Id),

        -- Rich Text Content
        OverviewHtml NVARCHAR(MAX) NULL,
        AdvantageHtml NVARCHAR(MAX) NULL,
        ApplicationRangeHtml NVARCHAR(MAX) NULL,
        PrecautionHtml NVARCHAR(MAX) NULL,

        -- JSON Storage for Structured Data
        SpecificationsJson NVARCHAR(MAX) NULL,
        TechnicalDataSheetsJson NVARCHAR(MAX) NULL,
        SafetyDataSheetsJson NVARCHAR(MAX) NULL,
        CertificatesJson NVARCHAR(MAX) NULL,
        
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    CREATE INDEX IX_Products_Name ON Products(Name);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductRelatedImages')
BEGIN
    CREATE TABLE ProductRelatedImages (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE CASCADE,
        ImageUrl NVARCHAR(500) NOT NULL
    );
END
GO

-- ============================================
-- Stored Procedures
-- ============================================

-- Master Data Procedures remains same as before... (already defined in previous step)

-- Product Procedures
CREATE OR ALTER PROCEDURE sp_CreateProduct
    @Name NVARCHAR(500),
    @ShortDescription NVARCHAR(MAX),
    @MainImage NVARCHAR(500),
    @CategoryId INT,
    @SubCategoryId INT,
    @BrandId INT,
    @UnitId INT,
    @CountryId INT,
    @OverviewHtml NVARCHAR(MAX),
    @AdvantageHtml NVARCHAR(MAX),
    @ApplicationRangeHtml NVARCHAR(MAX),
    @PrecautionHtml NVARCHAR(MAX),
    @SpecificationsJson NVARCHAR(MAX),
    @TechnicalDataSheetsJson NVARCHAR(MAX),
    @SafetyDataSheetsJson NVARCHAR(MAX),
    @CertificatesJson NVARCHAR(MAX),
    @RelatedImagesJson NVARCHAR(MAX),
    @NewProductId INT OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    INSERT INTO Products (
        Name, ShortDescription, MainImage, CategoryId, SubCategoryId, BrandId, UnitId, CountryId,
        OverviewHtml, AdvantageHtml, ApplicationRangeHtml, PrecautionHtml,
        SpecificationsJson, TechnicalDataSheetsJson, SafetyDataSheetsJson, CertificatesJson
    )
    VALUES (
        @Name, @ShortDescription, @MainImage, @CategoryId, @SubCategoryId, @BrandId, @UnitId, @CountryId,
        @OverviewHtml, @AdvantageHtml, @ApplicationRangeHtml, @PrecautionHtml,
        @SpecificationsJson, @TechnicalDataSheetsJson, @SafetyDataSheetsJson, @CertificatesJson
    );
    SET @NewProductId = SCOPE_IDENTITY();
    IF @RelatedImagesJson IS NOT NULL
        INSERT INTO ProductRelatedImages (ProductId, ImageUrl) SELECT @NewProductId, value FROM OPENJSON(@RelatedImagesJson);
    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateProduct
    @Id INT,
    @Name NVARCHAR(500),
    @ShortDescription NVARCHAR(MAX),
    @MainImage NVARCHAR(500),
    @CategoryId INT,
    @SubCategoryId INT,
    @BrandId INT,
    @UnitId INT,
    @CountryId INT,
    @OverviewHtml NVARCHAR(MAX),
    @AdvantageHtml NVARCHAR(MAX),
    @ApplicationRangeHtml NVARCHAR(MAX),
    @PrecautionHtml NVARCHAR(MAX),
    @SpecificationsJson NVARCHAR(MAX),
    @TechnicalDataSheetsJson NVARCHAR(MAX),
    @SafetyDataSheetsJson NVARCHAR(MAX),
    @CertificatesJson NVARCHAR(MAX),
    @RelatedImagesJson NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = @Id) THROW 50001, 'Product not found', 1;
    BEGIN TRANSACTION;
    UPDATE Products SET 
        Name = @Name, ShortDescription = @ShortDescription, MainImage = @MainImage,
        CategoryId = @CategoryId, SubCategoryId = @SubCategoryId, BrandId = @BrandId, UnitId = @UnitId, CountryId = @CountryId,
        OverviewHtml = @OverviewHtml, AdvantageHtml = @AdvantageHtml, ApplicationRangeHtml = @ApplicationRangeHtml, PrecautionHtml = @PrecautionHtml,
        SpecificationsJson = @SpecificationsJson, TechnicalDataSheetsJson = @TechnicalDataSheetsJson, SafetyDataSheetsJson = @SafetyDataSheetsJson, CertificatesJson = @CertificatesJson,
        UpdatedAt = GETUTCDATE() 
    WHERE Id = @Id;
    DELETE FROM ProductRelatedImages WHERE ProductId = @Id;
    IF @RelatedImagesJson IS NOT NULL
        INSERT INTO ProductRelatedImages (ProductId, ImageUrl) SELECT @Id, value FROM OPENJSON(@RelatedImagesJson);
    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE sp_GetProductById
    @Id INT
AS
BEGIN
    SELECT p.*, 
           c.Name AS CategoryName, 
           s.Name AS SubCategoryName, 
           b.Name AS BrandName, 
           u.Name AS UnitName, 
           co.Name AS CountryName
    FROM Products p
    LEFT JOIN CategoryMaster c ON p.CategoryId = c.Id
    LEFT JOIN SubCategoryMaster s ON p.SubCategoryId = s.Id
    LEFT JOIN BrandMaster b ON p.BrandId = b.Id
    LEFT JOIN UnitMaster u ON p.UnitId = u.Id
    LEFT JOIN CountryMaster co ON p.CountryId = co.Id
    WHERE p.Id = @Id;

    SELECT ImageUrl FROM ProductRelatedImages WHERE ProductId = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAllProducts
    @CategoryId INT = NULL,
    @BrandId INT = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SELECT p.*, 
           c.Name AS CategoryName, 
           s.Name AS SubCategoryName, 
           b.Name AS BrandName, 
           u.Name AS UnitName, 
           co.Name AS CountryName,
           COUNT(*) OVER() AS TotalCount
    FROM Products p
    LEFT JOIN CategoryMaster c ON p.CategoryId = c.Id
    LEFT JOIN SubCategoryMaster s ON p.SubCategoryId = s.Id
    LEFT JOIN BrandMaster b ON p.BrandId = b.Id
    LEFT JOIN UnitMaster u ON p.UnitId = u.Id
    LEFT JOIN CountryMaster co ON p.CountryId = co.Id
    WHERE (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@BrandId IS NULL OR p.BrandId = @BrandId)
      AND (@SearchTerm IS NULL OR p.Name LIKE '%' + @SearchTerm + '%' OR p.ShortDescription LIKE '%' + @SearchTerm + '%')
    ORDER BY p.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
