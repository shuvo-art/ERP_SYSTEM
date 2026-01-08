-- ============================================
-- Partners Module - Database Schema
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Partners')
BEGIN
    CREATE TABLE Partners (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        Slug NVARCHAR(200) NOT NULL UNIQUE,
        ShortDescription NVARCHAR(MAX) NULL,
        LongDescriptionTitle NVARCHAR(500) NULL,
        LongDescription NVARCHAR(MAX) NULL,
        LogoUrl NVARCHAR(500) NULL,
        BuildingImageUrl NVARCHAR(500) NULL,
        CompanyProfileJson NVARCHAR(MAX) NULL, -- JSON object
        ProductSegmentsJson NVARCHAR(MAX) NULL,  -- JSON array
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    CREATE INDEX IX_Partners_Name ON Partners(Name);
    CREATE INDEX IX_Partners_Slug ON Partners(Slug);
END
GO

-- 1. Create Partner
CREATE OR ALTER PROCEDURE sp_CreatePartner
    @Name NVARCHAR(200),
    @Slug NVARCHAR(200),
    @ShortDescription NVARCHAR(MAX),
    @LongDescriptionTitle NVARCHAR(500),
    @LongDescription NVARCHAR(MAX),
    @LogoUrl NVARCHAR(500),
    @BuildingImageUrl NVARCHAR(500),
    @CompanyProfileJson NVARCHAR(MAX),
    @ProductSegmentsJson NVARCHAR(MAX),
    @NewId INT OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    INSERT INTO Partners (Name, Slug, ShortDescription, LongDescriptionTitle, LongDescription, LogoUrl, BuildingImageUrl, CompanyProfileJson, ProductSegmentsJson)
    VALUES (@Name, @Slug, @ShortDescription, @LongDescriptionTitle, @LongDescription, @LogoUrl, @BuildingImageUrl, @CompanyProfileJson, @ProductSegmentsJson);
    SET @NewId = SCOPE_IDENTITY();
END
GO

-- 2. Update Partner
CREATE OR ALTER PROCEDURE sp_UpdatePartner
    @Id INT,
    @Name NVARCHAR(200),
    @Slug NVARCHAR(200),
    @ShortDescription NVARCHAR(MAX),
    @LongDescriptionTitle NVARCHAR(500),
    @LongDescription NVARCHAR(MAX),
    @LogoUrl NVARCHAR(500),
    @BuildingImageUrl NVARCHAR(500),
    @CompanyProfileJson NVARCHAR(MAX),
    @ProductSegmentsJson NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE Partners 
    SET Name = @Name,
        Slug = @Slug,
        ShortDescription = @ShortDescription,
        LongDescriptionTitle = @LongDescriptionTitle,
        LongDescription = @LongDescription,
        LogoUrl = @LogoUrl,
        BuildingImageUrl = @BuildingImageUrl,
        CompanyProfileJson = @CompanyProfileJson,
        ProductSegmentsJson = @ProductSegmentsJson,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END
GO

-- 3. Get Partner By Id
CREATE OR ALTER PROCEDURE sp_GetPartnerById
    @Id INT
AS
BEGIN
    SELECT * FROM Partners WHERE Id = @Id;
END
GO

-- 4. Get Partner By Slug
CREATE OR ALTER PROCEDURE sp_GetPartnerBySlug
    @Slug NVARCHAR(200)
AS
BEGIN
    SELECT * FROM Partners WHERE Slug = @Slug;
END
GO

-- 5. Get All Partners with Pagination and Search
CREATE OR ALTER PROCEDURE sp_GetPartners
    @Search NVARCHAR(200) = NULL,
    @Offset INT = 0,
    @Limit INT = 10
AS
BEGIN
    SELECT * FROM Partners
    WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%')
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;

    SELECT COUNT(*) FROM Partners
    WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%');
END
GO

-- 6. Delete Partner
CREATE OR ALTER PROCEDURE sp_DeletePartner
    @Id INT
AS
BEGIN
    DELETE FROM Partners WHERE Id = @Id;
END
GO
