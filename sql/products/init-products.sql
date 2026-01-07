-- ============================================
-- Products Module - Database Schema
-- ============================================

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
-- Stored Procedures for Product CRUD
-- ============================================

-- 1. Create Product
CREATE OR ALTER PROCEDURE sp_CreateProduct
    @Name NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @MainImage NVARCHAR(500),
    @Category NVARCHAR(100),
    @SubCategory NVARCHAR(100),
    @Brand NVARCHAR(100),
    @OverviewJson NVARCHAR(MAX),
    @ApplicationRange NVARCHAR(MAX),
    @RelatedImagesJson NVARCHAR(MAX),
    @AdvantagesJson NVARCHAR(MAX),
    @PrecautionsJson NVARCHAR(MAX),
    @DocumentsJson NVARCHAR(MAX),
    @NewProductId INT OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    INSERT INTO Products (
        Name, Description, MainImage, Category, SubCategory, Brand, 
        OverviewJson, ApplicationRange, AdvantagesJson, PrecautionsJson, DocumentsJson
    )
    VALUES (
        @Name, @Description, @MainImage, @Category, @SubCategory, @Brand, 
        @OverviewJson, @ApplicationRange, @AdvantagesJson, @PrecautionsJson, @DocumentsJson
    );

    SET @NewProductId = SCOPE_IDENTITY();

    -- Insert Related Images (Separate table for easier querying if needed)
    IF @RelatedImagesJson IS NOT NULL
    BEGIN
        INSERT INTO ProductRelatedImages (ProductId, ImageUrl)
        SELECT @NewProductId, value FROM OPENJSON(@RelatedImagesJson);
    END

    COMMIT TRANSACTION;
END
GO

-- 2. Update Product
CREATE OR ALTER PROCEDURE sp_UpdateProduct
    @Id INT,
    @Name NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @MainImage NVARCHAR(500),
    @Category NVARCHAR(100),
    @SubCategory NVARCHAR(100),
    @Brand NVARCHAR(100),
    @OverviewJson NVARCHAR(MAX),
    @ApplicationRange NVARCHAR(MAX),
    @RelatedImagesJson NVARCHAR(MAX),
    @AdvantagesJson NVARCHAR(MAX),
    @PrecautionsJson NVARCHAR(MAX),
    @DocumentsJson NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = @Id)
    BEGIN
        THROW 50001, 'Product not found', 1;
    END

    BEGIN TRANSACTION;

    UPDATE Products SET 
        Name = @Name,
        Description = @Description,
        MainImage = @MainImage,
        Category = @Category,
        SubCategory = @SubCategory,
        Brand = @Brand,
        OverviewJson = @OverviewJson,
        ApplicationRange = @ApplicationRange,
        AdvantagesJson = @AdvantagesJson,
        PrecautionsJson = @PrecautionsJson,
        DocumentsJson = @DocumentsJson,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;

    -- Refresh Related Images
    DELETE FROM ProductRelatedImages WHERE ProductId = @Id;
    IF @RelatedImagesJson IS NOT NULL
    BEGIN
        INSERT INTO ProductRelatedImages (ProductId, ImageUrl)
        SELECT @Id, value FROM OPENJSON(@RelatedImagesJson);
    END

    COMMIT TRANSACTION;
END
GO

-- 3. Get Product By Id
CREATE OR ALTER PROCEDURE sp_GetProductById
    @Id INT
AS
BEGIN
    -- Main details
    SELECT 
        Id, Name, Description, MainImage AS Image, Category, SubCategory, Brand, 
        OverviewJson, AdvantagesJson, PrecautionsJson, DocumentsJson, 
        ApplicationRange, CreatedAt, UpdatedAt
    FROM Products WHERE Id = @Id;

    -- Related Images
    SELECT ImageUrl FROM ProductRelatedImages WHERE ProductId = @Id;
END
GO

-- 4. Get All Products
CREATE OR ALTER PROCEDURE sp_GetAllProducts
AS
BEGIN
    SELECT 
        Id, Name, Description, MainImage AS Image, Category, SubCategory, Brand, 
        OverviewJson, AdvantagesJson, PrecautionsJson, DocumentsJson, 
        ApplicationRange, CreatedAt, UpdatedAt
    FROM Products 
    ORDER BY CreatedAt DESC;
END
GO

-- 5. Delete Product
CREATE OR ALTER PROCEDURE sp_DeleteProduct
    @Id INT
AS
BEGIN
    DELETE FROM Products WHERE Id = @Id;
END
GO
