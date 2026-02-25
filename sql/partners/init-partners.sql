-- ============================================
-- Partners Module - Database Schema
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Partners')
BEGIN
    CREATE TABLE Partners (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        Slug NVARCHAR(200) NOT NULL UNIQUE,
        ShortDescription NVARCHAR(MAX) NULL, -- Rich Text
        DetailsDescriptionTitle NVARCHAR(500) NULL,
        DetailsDescription NVARCHAR(MAX) NULL, -- Rich Text
        LogoUrl NVARCHAR(500) NULL,
        BuildingImageUrl NVARCHAR(500) NULL,
        VideoUrl NVARCHAR(500) NULL,
        
        -- Company Profile fields
        CompanyName NVARCHAR(200) NULL,
        BrandName NVARCHAR(200) NULL,
        EstablishedIn NVARCHAR(200) NULL,
        Website NVARCHAR(500) NULL,
        
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    CREATE INDEX IX_Partners_Name ON Partners(Name);
    CREATE INDEX IX_Partners_Slug ON Partners(Slug);
END
GO

-- Product Segments Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductSegments')
BEGIN
    CREATE TABLE ProductSegments (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PartnerId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        ImageUrl NVARCHAR(500) NULL,
        FOREIGN KEY (PartnerId) REFERENCES Partners(Id) ON DELETE CASCADE
    );
END
GO

-- Partner Documents Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PartnerDocuments')
BEGIN
    CREATE TABLE PartnerDocuments (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PartnerId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        DocumentUrl NVARCHAR(500) NULL,
        FOREIGN KEY (PartnerId) REFERENCES Partners(Id) ON DELETE CASCADE
    );
END
GO

-- 1. Create Partner
CREATE OR ALTER PROCEDURE sp_CreatePartner
    @Name NVARCHAR(200),
    @Slug NVARCHAR(200),
    @ShortDescription NVARCHAR(MAX),
    @DetailsDescriptionTitle NVARCHAR(500),
    @DetailsDescription NVARCHAR(MAX),
    @LogoUrl NVARCHAR(500),
    @BuildingImageUrl NVARCHAR(500),
    @VideoUrl NVARCHAR(500),
    @CompanyName NVARCHAR(200),
    @BrandName NVARCHAR(200),
    @EstablishedIn NVARCHAR(200),
    @Website NVARCHAR(500),
    @ProductSegmentsJson NVARCHAR(MAX), -- JSON string of [{ "Name": "...", "ImageUrl": "..." }]
    @DocumentsJson NVARCHAR(MAX),       -- JSON string of [{ "Name": "...", "DocumentUrl": "..." }]
    @NewId INT OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    
    INSERT INTO Partners (
        Name, Slug, ShortDescription, DetailsDescriptionTitle, DetailsDescription, 
        LogoUrl, BuildingImageUrl, VideoUrl, CompanyName, BrandName, EstablishedIn, 
        Website
    )
    VALUES (
        @Name, @Slug, @ShortDescription, @DetailsDescriptionTitle, @DetailsDescription, 
        @LogoUrl, @BuildingImageUrl, @VideoUrl, @CompanyName, @BrandName, @EstablishedIn, 
        @Website
    );
    
    SET @NewId = SCOPE_IDENTITY();

    -- Insert Product Segments from JSON
    IF @ProductSegmentsJson IS NOT NULL
    BEGIN
        INSERT INTO ProductSegments (PartnerId, Name, ImageUrl)
        SELECT @NewId, Name, ImageUrl
        FROM OPENJSON(@ProductSegmentsJson)
        WITH (
            Name NVARCHAR(200) '$.name',
            ImageUrl NVARCHAR(500) '$.image_url'
        );
    END

    -- Insert Documents from JSON
    IF @DocumentsJson IS NOT NULL
    BEGIN
        INSERT INTO PartnerDocuments (PartnerId, Name, DocumentUrl)
        SELECT @NewId, Name, DocumentUrl
        FROM OPENJSON(@DocumentsJson)
        WITH (
            Name NVARCHAR(200) '$.name',
            DocumentUrl NVARCHAR(500) '$.document_url'
        );
    END

    COMMIT TRANSACTION;
END
GO

-- 2. Update Partner
CREATE OR ALTER PROCEDURE sp_UpdatePartner
    @Id INT,
    @Name NVARCHAR(200),
    @Slug NVARCHAR(200),
    @ShortDescription NVARCHAR(MAX),
    @DetailsDescriptionTitle NVARCHAR(500),
    @DetailsDescription NVARCHAR(MAX),
    @LogoUrl NVARCHAR(500),
    @BuildingImageUrl NVARCHAR(500),
    @VideoUrl NVARCHAR(500),
    @CompanyName NVARCHAR(200),
    @BrandName NVARCHAR(200),
    @EstablishedIn NVARCHAR(200),
    @Website NVARCHAR(500),
    @ProductSegmentsJson NVARCHAR(MAX),
    @DocumentsJson NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    UPDATE Partners 
    SET Name = @Name,
        Slug = @Slug,
        ShortDescription = @ShortDescription,
        DetailsDescriptionTitle = @DetailsDescriptionTitle,
        DetailsDescription = @DetailsDescription,
        LogoUrl = @LogoUrl,
        BuildingImageUrl = @BuildingImageUrl,
        VideoUrl = @VideoUrl,
        CompanyName = @CompanyName,
        BrandName = @BrandName,
        EstablishedIn = @EstablishedIn,
        Website = @Website,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;

    -- Update segments: Simple approach - delete and recreate
    DELETE FROM ProductSegments WHERE PartnerId = @Id;
    IF @ProductSegmentsJson IS NOT NULL
    BEGIN
        INSERT INTO ProductSegments (PartnerId, Name, ImageUrl)
        SELECT @Id, Name, ImageUrl
        FROM OPENJSON(@ProductSegmentsJson)
        WITH (
            Name NVARCHAR(200) '$.name',
            ImageUrl NVARCHAR(500) '$.image_url'
        );
    END

    -- Update documents: Simple approach - delete and recreate
    DELETE FROM PartnerDocuments WHERE PartnerId = @Id;
    IF @DocumentsJson IS NOT NULL
    BEGIN
        INSERT INTO PartnerDocuments (PartnerId, Name, DocumentUrl)
        SELECT @Id, Name, DocumentUrl
        FROM OPENJSON(@DocumentsJson)
        WITH (
            Name NVARCHAR(200) '$.name',
            DocumentUrl NVARCHAR(500) '$.document_url'
        );
    END

    COMMIT TRANSACTION;
END
GO

-- 3. Get Partner By Id (including child tables)
CREATE OR ALTER PROCEDURE sp_GetPartnerById
    @Id INT
AS
BEGIN
    SELECT * FROM Partners WHERE Id = @Id;
    SELECT * FROM ProductSegments WHERE PartnerId = @Id;
    SELECT * FROM PartnerDocuments WHERE PartnerId = @Id;
END
GO

-- 4. Get Partner By Slug
CREATE OR ALTER PROCEDURE sp_GetPartnerBySlug
    @Slug NVARCHAR(200)
AS
BEGIN
    -- Declare PartnerId to find children
    DECLARE @PartnerId INT;
    SELECT @PartnerId = Id FROM Partners WHERE Slug = @Slug;

    SELECT * FROM Partners WHERE Id = @PartnerId;
    SELECT * FROM ProductSegments WHERE PartnerId = @PartnerId;
    SELECT * FROM PartnerDocuments WHERE PartnerId = @PartnerId;
END
GO

-- 5. Get All Partners (Summary list for listing page)
CREATE OR ALTER PROCEDURE sp_GetPartners
    @Search NVARCHAR(200) = NULL,
    @Offset INT = 0,
    @Limit INT = 10
AS
BEGIN
    SELECT * FROM Partners
    WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%' OR CompanyName LIKE '%' + @Search + '%' OR BrandName LIKE '%' + @Search + '%')
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;

    SELECT COUNT(*) FROM Partners
    WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%' OR CompanyName LIKE '%' + @Search + '%' OR BrandName LIKE '%' + @Search + '%');
END
GO

-- 6. Delete Partner (Handled by ON DELETE CASCADE)
CREATE OR ALTER PROCEDURE sp_DeletePartner
    @Id INT
AS
BEGIN
    DELETE FROM Partners WHERE Id = @Id;
END
GO
