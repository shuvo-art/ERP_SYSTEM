IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Master')
BEGIN
    CREATE DATABASE Master;
END
GO

USE Master;
GO

-- AboutUsSections Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AboutUsSections]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AboutUsSections] (
        [Id] NVARCHAR(50) PRIMARY KEY, -- 'about_us', 'mission', 'vision', etc.
        [Title] NVARCHAR(255) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [MetadataJson] NVARCHAR(MAX) NULL, -- Stores singleton fields (banner, video, pdfs)
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- AboutUsItems Table (Polymorphic List Items)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AboutUsItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AboutUsItems] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [SectionId] NVARCHAR(50) NOT NULL,
        [Title] NVARCHAR(255) NULL,
        [ShortDescription] NVARCHAR(MAX) NULL,
        [IconUrl] NVARCHAR(500) NULL,
        [ImageUrl] NVARCHAR(500) NULL,
        [Date] NVARCHAR(100) NULL,
        [Designation] NVARCHAR(255) NULL,
        [SocialLinksJson] NVARCHAR(MAX) NULL,
        [OrderIndex] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_AboutUsItems_Sections] FOREIGN KEY ([SectionId]) REFERENCES [dbo].[AboutUsSections]([Id]) ON DELETE CASCADE
    );
END
GO

-- Seed Sections
IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'about_us')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description, MetadataJson)
    VALUES ('about_us', 'About Us', 'TOCOMA Limited stands at the forefront as a premier supplier of quality products for the Concrete and Masonry industry in Dhaka, Bangladesh.', '{"banner_image_url": "https://example.com/about-us-banner-bridge.jpg"}');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'mission')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description)
    VALUES ('mission', 'Our Mission', 'TOCOMA is committed to becoming a leading supplier of construction chemicals and a technical pioneer in Bangladesh''s construction industry.');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'vision')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description)
    VALUES ('vision', 'Our Vision', 'To work at the grassroots level and engage in the construction industry as a technical pioneer in Bangladesh.');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'core_values')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description)
    VALUES ('core_values', 'Core Values', 'At TOCOMA, our five core values guide our commitment to excellence and innovation.');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'customer_solutions')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description)
    VALUES ('customer_solutions', 'Customer Solutions', 'At TOCOMA, we provide top-notch products, outstanding customer service, and strong industry support through highly trained teams.');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'business_principles')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description)
    VALUES ('business_principles', 'Business Principles', 'At TOCOMA, our five business principles are the cornerstone of our dedication to quality, integrity, and innovation.');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'video')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description, MetadataJson)
    VALUES ('video', 'Video', 'Watch our video to learn more about TOCOMA''s journey and commitment.', '{"thumbnail_url": "", "video_url": ""}');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'journey_milestones')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description)
    VALUES ('journey_milestones', 'Our Journey & Key Milestones', 'Since our inception in 2013, TOCOMA Limited has steadily progressed through key milestones.');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'team')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description)
    VALUES ('team', 'The Team Driving Our Success', 'At TOCOMA, our success is driven by a team of dedicated experts, committed to delivering innovative solutions.');
END

IF NOT EXISTS (SELECT * FROM [dbo].[AboutUsSections] WHERE Id = 'quick_reference')
BEGIN
    INSERT INTO [dbo].[AboutUsSections] (Id, Title, Description, MetadataJson)
    VALUES ('quick_reference', 'Quick Reference', 'For a Quick Snapshot of TOCOMA''s Offerings, Download Our Company Profile And Product Brochure.', '{"company_profile_pdf_url": "", "product_brochure_pdf_url": ""}');
END
GO

-- Stored Procedures

-- sp_GetAboutUs (Full Page)
CREATE OR ALTER PROCEDURE sp_GetAboutUs
AS
BEGIN
    SELECT * FROM [dbo].[AboutUsSections];
    SELECT * FROM [dbo].[AboutUsItems] ORDER BY SectionId, OrderIndex;
END
GO

-- sp_UpdateSection
CREATE OR ALTER PROCEDURE sp_UpdateSection
    @Id NVARCHAR(50),
    @Title NVARCHAR(255),
    @Description NVARCHAR(MAX),
    @MetadataJson NVARCHAR(MAX)
AS
BEGIN
    UPDATE [dbo].[AboutUsSections]
    SET Title = @Title,
        Description = @Description,
        MetadataJson = @MetadataJson,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END
GO

-- sp_AddAboutUsItem
CREATE OR ALTER PROCEDURE sp_AddAboutUsItem
    @SectionId NVARCHAR(50),
    @Title NVARCHAR(255) = NULL,
    @ShortDescription NVARCHAR(MAX) = NULL,
    @IconUrl NVARCHAR(500) = NULL,
    @ImageUrl NVARCHAR(500) = NULL,
    @Date NVARCHAR(100) = NULL,
    @Designation NVARCHAR(255) = NULL,
    @SocialLinksJson NVARCHAR(MAX) = NULL,
    @OrderIndex INT = 0,
    @NewItemId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET @NewItemId = NEWID();
    INSERT INTO [dbo].[AboutUsItems] (Id, SectionId, Title, ShortDescription, IconUrl, ImageUrl, Date, Designation, SocialLinksJson, OrderIndex)
    VALUES (@NewItemId, @SectionId, @Title, @ShortDescription, @IconUrl, @ImageUrl, @Date, @Designation, @SocialLinksJson, @OrderIndex);
END
GO

-- sp_UpdateAboutUsItem
CREATE OR ALTER PROCEDURE sp_UpdateAboutUsItem
    @Id UNIQUEIDENTIFIER,
    @Title NVARCHAR(255) = NULL,
    @ShortDescription NVARCHAR(MAX) = NULL,
    @IconUrl NVARCHAR(500) = NULL,
    @ImageUrl NVARCHAR(500) = NULL,
    @Date NVARCHAR(100) = NULL,
    @Designation NVARCHAR(255) = NULL,
    @SocialLinksJson NVARCHAR(MAX) = NULL,
    @OrderIndex INT = 0
AS
BEGIN
    UPDATE [dbo].[AboutUsItems]
    SET Title = @Title,
        ShortDescription = @ShortDescription,
        IconUrl = @IconUrl,
        ImageUrl = @ImageUrl,
        Date = @Date,
        Designation = @Designation,
        SocialLinksJson = @SocialLinksJson,
        OrderIndex = @OrderIndex,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END
GO

-- sp_DeleteAboutUsItem
CREATE OR ALTER PROCEDURE sp_DeleteAboutUsItem
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    DELETE FROM [dbo].[AboutUsItems] WHERE Id = @Id;
END
GO
