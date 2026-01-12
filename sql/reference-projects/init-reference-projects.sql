-- ============================================
-- Reference Projects Module - Database Schema
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReferenceProjects')
BEGIN
    CREATE TABLE ReferenceProjects (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ProjectName NVARCHAR(255) NOT NULL,
        Slug NVARCHAR(255) NOT NULL UNIQUE,
        ShortDescription NVARCHAR(MAX) NOT NULL,
        HeroImageUrl NVARCHAR(MAX) NULL,
        GalleryImagesJson NVARCHAR(MAX) NULL,
        Location NVARCHAR(MAX) NOT NULL,
        ProjectOverviewJson NVARCHAR(MAX) NULL,
        ProductsUsedJson NVARCHAR(MAX) NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'ongoing',
        StartDate DATETIME2 NULL,
        CompletionDate DATETIME2 NULL,
        Featured BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_ReferenceProjects_Slug ON ReferenceProjects(Slug);
    CREATE INDEX IX_ReferenceProjects_Status ON ReferenceProjects(Status);
    CREATE INDEX IX_ReferenceProjects_Featured ON ReferenceProjects(Featured);
END
GO

-- ============================================
-- Stored Procedures for Reference Project CRUD
-- ============================================

-- 1. Create Reference Project
CREATE OR ALTER PROCEDURE sp_CreateReferenceProject
    @ProjectName NVARCHAR(255),
    @Slug NVARCHAR(255),
    @ShortDescription NVARCHAR(MAX),
    @HeroImageUrl NVARCHAR(MAX),
    @GalleryImagesJson NVARCHAR(MAX),
    @Location NVARCHAR(MAX),
    @ProjectOverviewJson NVARCHAR(MAX),
    @ProductsUsedJson NVARCHAR(MAX),
    @Status NVARCHAR(50),
    @StartDate DATETIME2,
    @CompletionDate DATETIME2,
    @Featured BIT,
    @NewProjectId INT OUTPUT
AS
BEGIN
    INSERT INTO ReferenceProjects (
        ProjectName, Slug, ShortDescription, HeroImageUrl, GalleryImagesJson, 
        Location, ProjectOverviewJson, ProductsUsedJson, Status, 
        StartDate, CompletionDate, Featured
    )
    VALUES (
        @ProjectName, @Slug, @ShortDescription, @HeroImageUrl, @GalleryImagesJson,
        @Location, @ProjectOverviewJson, @ProductsUsedJson, @Status,
        @StartDate, @CompletionDate, @Featured
    );

    SET @NewProjectId = SCOPE_IDENTITY();
END
GO

-- 2. Update Reference Project
CREATE OR ALTER PROCEDURE sp_UpdateReferenceProject
    @Id INT,
    @ProjectName NVARCHAR(255),
    @Slug NVARCHAR(255),
    @ShortDescription NVARCHAR(MAX),
    @HeroImageUrl NVARCHAR(MAX),
    @GalleryImagesJson NVARCHAR(MAX),
    @Location NVARCHAR(MAX),
    @ProjectOverviewJson NVARCHAR(MAX),
    @ProductsUsedJson NVARCHAR(MAX),
    @Status NVARCHAR(50),
    @StartDate DATETIME2,
    @CompletionDate DATETIME2,
    @Featured BIT
AS
BEGIN
    UPDATE ReferenceProjects SET
        ProjectName = @ProjectName,
        Slug = @Slug,
        ShortDescription = @ShortDescription,
        HeroImageUrl = @HeroImageUrl,
        GalleryImagesJson = @GalleryImagesJson,
        Location = @Location,
        ProjectOverviewJson = @ProjectOverviewJson,
        ProductsUsedJson = @ProductsUsedJson,
        Status = @Status,
        StartDate = @StartDate,
        CompletionDate = @CompletionDate,
        Featured = @Featured,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END
GO

-- 3. Get All Reference Projects (with Pagination and Filters)
CREATE OR ALTER PROCEDURE sp_GetReferenceProjects
    @Page INT = 1,
    @Limit INT = 10,
    @Status NVARCHAR(50) = NULL,
    @Featured BIT = NULL,
    @Search NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @Limit;

    SELECT * FROM ReferenceProjects
    WHERE (@Status IS NULL OR Status = @Status)
    AND (@Featured IS NULL OR Featured = @Featured)
    AND (@Search IS NULL OR ProjectName LIKE '%' + @Search + '%' OR ShortDescription LIKE '%' + @Search + '%')
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;

    -- Get total count for pagination
    SELECT COUNT(*) FROM ReferenceProjects
    WHERE (@Status IS NULL OR Status = @Status)
    AND (@Featured IS NULL OR Featured = @Featured)
    AND (@Search IS NULL OR ProjectName LIKE '%' + @Search + '%' OR ShortDescription LIKE '%' + @Search + '%');
END
GO

-- 4. Get Reference Project By ID
CREATE OR ALTER PROCEDURE sp_GetReferenceProjectById
    @Id INT
AS
BEGIN
    SELECT * FROM ReferenceProjects WHERE Id = @Id;
END
GO

-- 5. Get Reference Project By Slug
CREATE OR ALTER PROCEDURE sp_GetReferenceProjectBySlug
    @Slug NVARCHAR(255)
AS
BEGIN
    SELECT * FROM ReferenceProjects WHERE Slug = @Slug;
END
GO

-- 6. Delete Reference Project
CREATE OR ALTER PROCEDURE sp_DeleteReferenceProject
    @Id INT
AS
BEGIN
    DELETE FROM ReferenceProjects WHERE Id = @Id;
END
GO
