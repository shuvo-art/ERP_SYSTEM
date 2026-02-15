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
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        MainImage NVARCHAR(500) NULL,
        Category NVARCHAR(100) NULL,
        SubCategory NVARCHAR(100) NULL,
        Brand NVARCHAR(100) NULL,
        
        -- Combined JSON storage for complex nested structures
        OverviewJson NVARCHAR(MAX) NULL,   -- Contains { details, specifications: [] }
        AdvantagesJson NVARCHAR(MAX) NULL, -- Array of strings
        PrecautionsJson NVARCHAR(MAX) NULL, -- Array of strings
        DocumentsJson NVARCHAR(MAX) NULL,   -- Object with arrays for each type
        
        ApplicationRange NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    CREATE INDEX IX_Products_Name ON Products(Name);
    CREATE INDEX IX_Products_Category ON Products(Category);
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

-- Master Data Procedures
CREATE OR ALTER PROCEDURE sp_ManageCategory
    @Action NVARCHAR(20), @Id INT = NULL, @Name NVARCHAR(255) = NULL, @Image NVARCHAR(500) = NULL
AS
BEGIN
    IF @Action = 'CREATE' INSERT INTO CategoryMaster (Name, Image) VALUES (@Name, @Image);
    ELSE IF @Action = 'UPDATE' UPDATE CategoryMaster SET Name = @Name, Image = @Image WHERE Id = @Id;
    ELSE IF @Action = 'DELETE' DELETE FROM CategoryMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' SELECT * FROM CategoryMaster WHERE (@Id IS NULL OR Id = @Id);
END
GO

CREATE OR ALTER PROCEDURE sp_ManageSubCategory
    @Action NVARCHAR(20), @Id INT = NULL, @CategoryId INT = NULL, @Name NVARCHAR(255) = NULL
AS
BEGIN
    IF @Action = 'CREATE' INSERT INTO SubCategoryMaster (CategoryId, Name) VALUES (@CategoryId, @Name);
    ELSE IF @Action = 'UPDATE' UPDATE SubCategoryMaster SET CategoryId = @CategoryId, Name = @Name WHERE Id = @Id;
    ELSE IF @Action = 'DELETE' DELETE FROM SubCategoryMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' 
        SELECT s.*, c.Name AS CategoryName FROM SubCategoryMaster s 
        JOIN CategoryMaster c ON s.CategoryId = c.Id
        WHERE (@Id IS NULL OR s.Id = @Id);
END
GO

CREATE OR ALTER PROCEDURE sp_ManageBrand
    @Action NVARCHAR(20), @Id INT = NULL, @Name NVARCHAR(255) = NULL, @Logo NVARCHAR(500) = NULL
AS
BEGIN
    IF @Action = 'CREATE' INSERT INTO BrandMaster (Name, Logo) VALUES (@Name, @Logo);
    ELSE IF @Action = 'UPDATE' UPDATE BrandMaster SET Name = @Name, Logo = @Logo WHERE Id = @Id;
    ELSE IF @Action = 'DELETE' DELETE FROM BrandMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' SELECT * FROM BrandMaster WHERE (@Id IS NULL OR Id = @Id);
END
GO

CREATE OR ALTER PROCEDURE sp_ManageUnit
    @Action NVARCHAR(20), @Id INT = NULL, @Name NVARCHAR(50) = NULL
AS
BEGIN
    IF @Action = 'CREATE' INSERT INTO UnitMaster (Name) VALUES (@Name);
    ELSE IF @Action = 'UPDATE' UPDATE UnitMaster SET Name = @Name WHERE Id = @Id;
    ELSE IF @Action = 'DELETE' DELETE FROM UnitMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' SELECT * FROM UnitMaster WHERE (@Id IS NULL OR Id = @Id);
END
GO

CREATE OR ALTER PROCEDURE sp_ManageCountry
    @Action NVARCHAR(20), @Id INT = NULL, @Name NVARCHAR(255) = NULL
AS
BEGIN
    IF @Action = 'CREATE' INSERT INTO CountryMaster (Name) VALUES (@Name);
    ELSE IF @Action = 'UPDATE' UPDATE CountryMaster SET Name = @Name WHERE Id = @Id;
    ELSE IF @Action = 'DELETE' DELETE FROM CountryMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' SELECT * FROM CountryMaster WHERE (@Id IS NULL OR Id = @Id);
END
GO

-- Product Procedures
CREATE OR ALTER PROCEDURE sp_CreateProduct
    @Name NVARCHAR(200), @Description NVARCHAR(MAX), @MainImage NVARCHAR(500), 
    @Category NVARCHAR(100), @SubCategory NVARCHAR(100), @Brand NVARCHAR(100),
    @OverviewJson NVARCHAR(MAX), @ApplicationRange NVARCHAR(MAX), @RelatedImagesJson NVARCHAR(MAX),
    @AdvantagesJson NVARCHAR(MAX), @PrecautionsJson NVARCHAR(MAX), @DocumentsJson NVARCHAR(MAX),
    @NewProductId INT OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    INSERT INTO Products (Name, Description, MainImage, Category, SubCategory, Brand, OverviewJson, ApplicationRange, AdvantagesJson, PrecautionsJson, DocumentsJson)
    VALUES (@Name, @Description, @MainImage, @Category, @SubCategory, @Brand, @OverviewJson, @ApplicationRange, @AdvantagesJson, @PrecautionsJson, @DocumentsJson);
    SET @NewProductId = SCOPE_IDENTITY();
    IF @RelatedImagesJson IS NOT NULL
        INSERT INTO ProductRelatedImages (ProductId, ImageUrl) SELECT @NewProductId, value FROM OPENJSON(@RelatedImagesJson);
    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateProduct
    @Id INT, @Name NVARCHAR(200), @Description NVARCHAR(MAX), @MainImage NVARCHAR(500),
    @Category NVARCHAR(100), @SubCategory NVARCHAR(100), @Brand NVARCHAR(100),
    @OverviewJson NVARCHAR(MAX), @ApplicationRange NVARCHAR(MAX), @RelatedImagesJson NVARCHAR(MAX),
    @AdvantagesJson NVARCHAR(MAX), @PrecautionsJson NVARCHAR(MAX), @DocumentsJson NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = @Id) THROW 50001, 'Product not found', 1;
    BEGIN TRANSACTION;
    UPDATE Products SET Name = @Name, Description = @Description, MainImage = @MainImage, Category = @Category, SubCategory = @SubCategory, Brand = @Brand, OverviewJson = @OverviewJson, ApplicationRange = @ApplicationRange, AdvantagesJson = @AdvantagesJson, PrecautionsJson = @PrecautionsJson, DocumentsJson = @DocumentsJson, UpdatedAt = GETUTCDATE() WHERE Id = @Id;
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
    SELECT Id, Name, Description, MainImage AS Image, Category, SubCategory, Brand, OverviewJson, AdvantagesJson, PrecautionsJson, DocumentsJson, ApplicationRange, CreatedAt, UpdatedAt FROM Products WHERE Id = @Id;
    SELECT ImageUrl FROM ProductRelatedImages WHERE ProductId = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAllProducts
AS
BEGIN
    SELECT Id, Name, Description, MainImage AS Image, Category, SubCategory, Brand, OverviewJson, AdvantagesJson, PrecautionsJson, DocumentsJson, ApplicationRange, CreatedAt, UpdatedAt FROM Products ORDER BY CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_DeleteProduct @Id INT AS DELETE FROM Products WHERE Id = @Id;
GO
