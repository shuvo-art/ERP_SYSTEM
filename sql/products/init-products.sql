-- ============================================
-- Products Module - Database Schema
-- ============================================

-- 1. Master Tables
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryMaster')
BEGIN
    CREATE TABLE CategoryMaster (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL UNIQUE,
        Slug NVARCHAR(255) NOT NULL DEFAULT '',
        Image NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
ELSE IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CategoryMaster') AND name = 'Slug')
BEGIN
    ALTER TABLE CategoryMaster ADD Slug NVARCHAR(255) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubCategoryMaster')
BEGIN
    CREATE TABLE SubCategoryMaster (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        Slug NVARCHAR(255) NOT NULL DEFAULT '',
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
ELSE 
BEGIN
    -- Check if Slug column exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SubCategoryMaster') AND name = 'Slug')
    BEGIN
        ALTER TABLE SubCategoryMaster ADD Slug NVARCHAR(255) NOT NULL DEFAULT '';
    END

    -- DROP CategoryId if it exists (for many-to-many migration)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SubCategoryMaster') AND name = 'CategoryId')
    BEGIN
        DECLARE @ConstraintName NVARCHAR(MAX);
        SELECT @ConstraintName = fk.name
        FROM sys.foreign_keys AS fk
        INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.columns AS c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
        WHERE fk.parent_object_id = OBJECT_ID('SubCategoryMaster') AND c.name = 'CategoryId';

        IF @ConstraintName IS NOT NULL
        BEGIN
            EXEC('ALTER TABLE SubCategoryMaster DROP CONSTRAINT ' + @ConstraintName);
        END
        
        ALTER TABLE SubCategoryMaster DROP COLUMN CategoryId;
    END
END

-- Many-to-Many Join Table for Categories and Sub-Categories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategorySubCategoryRel')
BEGIN
    CREATE TABLE CategorySubCategoryRel (
        CategoryId INT NOT NULL FOREIGN KEY REFERENCES CategoryMaster(Id) ON DELETE CASCADE,
        SubCategoryId INT NOT NULL FOREIGN KEY REFERENCES SubCategoryMaster(Id) ON DELETE CASCADE,
        PRIMARY KEY (CategoryId, SubCategoryId)
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BrandMaster')
BEGIN
    CREATE TABLE BrandMaster (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        Slug NVARCHAR(255) NOT NULL DEFAULT '',
        Logo NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
ELSE IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BrandMaster') AND name = 'Slug')
BEGIN
    ALTER TABLE BrandMaster ADD Slug NVARCHAR(255) NOT NULL DEFAULT '';
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
        Slug NVARCHAR(500) NOT NULL DEFAULT '',
        ShortDescription NVARCHAR(MAX) NULL,
        MainImage NVARCHAR(500) NULL,
        CategoryId INT NULL FOREIGN KEY REFERENCES CategoryMaster(Id),
        SubCategoryId INT NULL FOREIGN KEY REFERENCES SubCategoryMaster(Id),
        BrandId INT NULL FOREIGN KEY REFERENCES BrandMaster(Id),
        UnitId INT NULL FOREIGN KEY REFERENCES UnitMaster(Id),
        CountryId INT NULL FOREIGN KEY REFERENCES CountryMaster(Id),
        OverviewHtml NVARCHAR(MAX) NULL,
        AdvantageHtml NVARCHAR(MAX) NULL,
        ApplicationRangeHtml NVARCHAR(MAX) NULL,
        PrecautionHtml NVARCHAR(MAX) NULL,
        SpecificationsJson NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    CREATE INDEX IX_Products_Name ON Products(Name);
END
ELSE
BEGIN
    -- Cleanup old JSON columns if they exist
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'TechnicalDataSheetsJson')
        ALTER TABLE Products DROP COLUMN TechnicalDataSheetsJson;
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'SafetyDataSheetsJson')
        ALTER TABLE Products DROP COLUMN SafetyDataSheetsJson;
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'CertificatesJson')
        ALTER TABLE Products DROP COLUMN CertificatesJson;
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

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductTechnicalDataSheets')
BEGIN
    CREATE TABLE ProductTechnicalDataSheets (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE CASCADE,
        Name NVARCHAR(500) NOT NULL,
        Url NVARCHAR(500) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductSafetyDataSheets')
BEGIN
    CREATE TABLE ProductSafetyDataSheets (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE CASCADE,
        Name NVARCHAR(500) NOT NULL,
        Url NVARCHAR(500) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductCertificates')
BEGIN
    CREATE TABLE ProductCertificates (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE CASCADE,
        Name NVARCHAR(500) NOT NULL,
        Url NVARCHAR(500) NOT NULL
    );
END
GO

-- ============================================
-- Stored Procedures
-- ============================================

-- Master Data Procedures
CREATE OR ALTER PROCEDURE sp_ManageCategory
    @Action NVARCHAR(20), 
    @Id INT = NULL, 
    @Name NVARCHAR(255) = NULL, 
    @Slug NVARCHAR(255) = NULL,
    @Image NVARCHAR(500) = NULL,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    IF @Action = 'CREATE' 
    BEGIN
        INSERT INTO CategoryMaster (Name, Slug, Image) VALUES (@Name, @Slug, @Image);
        SELECT CAST(SCOPE_IDENTITY() as int);
    END
    ELSE IF @Action = 'UPDATE' UPDATE CategoryMaster SET Name = @Name, Slug = @Slug, Image = @Image WHERE Id = @Id;
    ELSE IF @Action = 'DELETE' DELETE FROM CategoryMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' 
        SELECT * FROM CategoryMaster 
        WHERE (@Id IS NULL OR Id = @Id)
          AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%')
          AND (@Slug IS NULL OR Slug = @Slug);
END
GO

CREATE OR ALTER PROCEDURE sp_ManageSubCategory
    @Action NVARCHAR(20), 
    @Id INT = NULL, 
    @Name NVARCHAR(255) = NULL, 
    @Slug NVARCHAR(255) = NULL,
    @SearchTerm NVARCHAR(100) = NULL,
    @CategoryIdsJson NVARCHAR(MAX) = NULL -- JSON array of category IDs
AS
BEGIN
    IF @Action = 'CREATE'
    BEGIN
        INSERT INTO SubCategoryMaster (Name, Slug) VALUES (@Name, @Slug);
        SET @Id = SCOPE_IDENTITY();
        
        IF @CategoryIdsJson IS NOT NULL
            INSERT INTO CategorySubCategoryRel (CategoryId, SubCategoryId)
            SELECT value, @Id FROM OPENJSON(@CategoryIdsJson);
            
        SELECT CAST(@Id as int);
    END
    ELSE IF @Action = 'UPDATE'
    BEGIN
        UPDATE SubCategoryMaster SET Name = @Name, Slug = @Slug WHERE Id = @Id;
        
        IF @CategoryIdsJson IS NOT NULL
        BEGIN
            DELETE FROM CategorySubCategoryRel WHERE SubCategoryId = @Id;
            INSERT INTO CategorySubCategoryRel (CategoryId, SubCategoryId)
            SELECT value, @Id FROM OPENJSON(@CategoryIdsJson);
        END
    END
    ELSE IF @Action = 'DELETE' DELETE FROM SubCategoryMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' 
    BEGIN
        SELECT s.* FROM SubCategoryMaster s
        WHERE (@Id IS NULL OR s.Id = @Id)
          AND (@SearchTerm IS NULL OR s.Name LIKE '%' + @SearchTerm + '%')
          AND (@Slug IS NULL OR s.Slug = @Slug);
        
        -- Return all relevant mappings in a second result set
        SELECT CategoryId, SubCategoryId FROM CategorySubCategoryRel
        WHERE (@Id IS NULL OR SubCategoryId = @Id);
    END
    ELSE IF @Action = 'GET_BY_CATEGORY'
    BEGIN
        SELECT s.* FROM SubCategoryMaster s
        JOIN CategorySubCategoryRel r ON s.Id = r.SubCategoryId
        WHERE r.CategoryId = (SELECT TOP 1 Id FROM CategoryMaster WHERE Id = @Id OR Slug = @Slug);
        
        -- Return mappings for these subcategories too
        SELECT r.CategoryId, r.SubCategoryId FROM CategorySubCategoryRel r
        JOIN CategorySubCategoryRel filter ON r.SubCategoryId = filter.SubCategoryId
        WHERE filter.CategoryId = (SELECT TOP 1 Id FROM CategoryMaster WHERE Id = @Id OR Slug = @Slug);
    END
END
GO

CREATE OR ALTER PROCEDURE sp_ManageBrand
    @Action NVARCHAR(20), 
    @Id INT = NULL, 
    @Name NVARCHAR(255) = NULL, 
    @Slug NVARCHAR(255) = NULL,
    @Logo NVARCHAR(500) = NULL,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    IF @Action = 'CREATE'
    BEGIN
        INSERT INTO BrandMaster (Name, Slug, Logo) VALUES (@Name, @Slug, @Logo);
        SELECT CAST(SCOPE_IDENTITY() as int);
    END
    ELSE IF @Action = 'UPDATE' UPDATE BrandMaster SET Name = @Name, Slug = @Slug, Logo = @Logo WHERE Id = @Id;
    ELSE IF @Action = 'DELETE' DELETE FROM BrandMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' 
        SELECT * FROM BrandMaster 
        WHERE (@Id IS NULL OR Id = @Id)
          AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%')
          AND (@Slug IS NULL OR Slug = @Slug);
END
GO

CREATE OR ALTER PROCEDURE sp_ManageUnit
    @Action NVARCHAR(20), @Id INT = NULL, @Name NVARCHAR(50) = NULL, @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    IF @Action = 'CREATE' 
    BEGIN
        INSERT INTO UnitMaster (Name) VALUES (@Name);
        SELECT CAST(SCOPE_IDENTITY() as int);
    END
    ELSE IF @Action = 'UPDATE' UPDATE UnitMaster SET Name = @Name WHERE Id = @Id;
    ELSE IF @Action = 'DELETE' DELETE FROM UnitMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' 
        SELECT * FROM UnitMaster 
        WHERE (@Id IS NULL OR Id = @Id)
          AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%');
END
GO

CREATE OR ALTER PROCEDURE sp_ManageCountry
    @Action NVARCHAR(20), @Id INT = NULL, @Name NVARCHAR(255) = NULL, @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    IF @Action = 'CREATE' 
    BEGIN
        INSERT INTO CountryMaster (Name) VALUES (@Name);
        SELECT CAST(SCOPE_IDENTITY() as int);
    END
    ELSE IF @Action = 'UPDATE' UPDATE CountryMaster SET Name = @Name WHERE Id = @Id;
    ELSE IF @Action = 'DELETE' DELETE FROM CountryMaster WHERE Id = @Id;
    ELSE IF @Action = 'GET' 
        SELECT * FROM CountryMaster 
        WHERE (@Id IS NULL OR Id = @Id)
          AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%');
END
GO

-- Product Procedures
CREATE OR ALTER PROCEDURE sp_CreateProduct
    @Name NVARCHAR(500),
    @Slug NVARCHAR(500),
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
        Name, Slug, ShortDescription, MainImage, CategoryId, SubCategoryId, BrandId, UnitId, CountryId,
        OverviewHtml, AdvantageHtml, ApplicationRangeHtml, PrecautionHtml,
        SpecificationsJson
    )
    VALUES (
        @Name, @Slug, @ShortDescription, @MainImage, @CategoryId, @SubCategoryId, @BrandId, @UnitId, @CountryId,
        @OverviewHtml, @AdvantageHtml, @ApplicationRangeHtml, @PrecautionHtml,
        @SpecificationsJson
    );
    SET @NewProductId = SCOPE_IDENTITY();
    
    -- Insert Related Images
    IF @RelatedImagesJson IS NOT NULL
        INSERT INTO ProductRelatedImages (ProductId, ImageUrl) SELECT @NewProductId, value FROM OPENJSON(@RelatedImagesJson);
    
    -- Insert TDS
    IF @TechnicalDataSheetsJson IS NOT NULL
        INSERT INTO ProductTechnicalDataSheets (ProductId, Name, Url) 
        SELECT @NewProductId, JSON_VALUE(value, '$.Name'), JSON_VALUE(value, '$.Url') FROM OPENJSON(@TechnicalDataSheetsJson);
    
    -- Insert SDS
    IF @SafetyDataSheetsJson IS NOT NULL
        INSERT INTO ProductSafetyDataSheets (ProductId, Name, Url) 
        SELECT @NewProductId, JSON_VALUE(value, '$.Name'), JSON_VALUE(value, '$.Url') FROM OPENJSON(@SafetyDataSheetsJson);
    
    -- Insert Certificates
    IF @CertificatesJson IS NOT NULL
        INSERT INTO ProductCertificates (ProductId, Name, Url) 
        SELECT @NewProductId, JSON_VALUE(value, '$.Name'), JSON_VALUE(value, '$.Url') FROM OPENJSON(@CertificatesJson);
        
    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateProduct
    @Id INT,
    @Name NVARCHAR(500),
    @Slug NVARCHAR(500),
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
        Name = @Name, Slug = @Slug, ShortDescription = @ShortDescription, MainImage = @MainImage,
        CategoryId = @CategoryId, SubCategoryId = @SubCategoryId, BrandId = @BrandId, UnitId = @UnitId, CountryId = @CountryId,
        OverviewHtml = @OverviewHtml, AdvantageHtml = @AdvantageHtml, ApplicationRangeHtml = @ApplicationRangeHtml, PrecautionHtml = @PrecautionHtml,
        SpecificationsJson = @SpecificationsJson,
        UpdatedAt = GETUTCDATE() 
    WHERE Id = @Id;

    -- Cleanup and Re-insert
    DELETE FROM ProductRelatedImages WHERE ProductId = @Id;
    IF @RelatedImagesJson IS NOT NULL
        INSERT INTO ProductRelatedImages (ProductId, ImageUrl) SELECT @Id, value FROM OPENJSON(@RelatedImagesJson);

    DELETE FROM ProductTechnicalDataSheets WHERE ProductId = @Id;
    IF @TechnicalDataSheetsJson IS NOT NULL
        INSERT INTO ProductTechnicalDataSheets (ProductId, Name, Url) 
        SELECT @Id, JSON_VALUE(value, '$.Name'), JSON_VALUE(value, '$.Url') FROM OPENJSON(@TechnicalDataSheetsJson);

    DELETE FROM ProductSafetyDataSheets WHERE ProductId = @Id;
    IF @SafetyDataSheetsJson IS NOT NULL
        INSERT INTO ProductSafetyDataSheets (ProductId, Name, Url) 
        SELECT @Id, JSON_VALUE(value, '$.Name'), JSON_VALUE(value, '$.Url') FROM OPENJSON(@SafetyDataSheetsJson);

    DELETE FROM ProductCertificates WHERE ProductId = @Id;
    IF @CertificatesJson IS NOT NULL
        INSERT INTO ProductCertificates (ProductId, Name, Url) 
        SELECT @Id, JSON_VALUE(value, '$.Name'), JSON_VALUE(value, '$.Url') FROM OPENJSON(@CertificatesJson);

    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE sp_DeleteProduct
    @Id INT
AS
BEGIN
    DELETE FROM Products WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_GetProductById
    @Id INT = NULL,
    @Slug NVARCHAR(500) = NULL
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
    WHERE (@Id IS NULL OR p.Id = @Id)
      AND (@Slug IS NULL OR p.Slug = @Slug);

    DECLARE @ActualId INT;
    IF @Id IS NOT NULL SET @ActualId = @Id;
    ELSE SELECT @ActualId = Id FROM Products WHERE Slug = @Slug;

    IF @ActualId IS NOT NULL
    BEGIN
        SELECT ImageUrl FROM ProductRelatedImages WHERE ProductId = @ActualId;
        SELECT Name, Url FROM ProductTechnicalDataSheets WHERE ProductId = @ActualId;
        SELECT Name, Url FROM ProductSafetyDataSheets WHERE ProductId = @ActualId;
        SELECT Name, Url FROM ProductCertificates WHERE ProductId = @ActualId;
    END
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
