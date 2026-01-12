IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Master')
BEGIN
    CREATE DATABASE Master;
END
GO

USE Master;
GO

-- JobPostings Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JobPostings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[JobPostings] (
        [Id] Guid PRIMARY KEY DEFAULT NEWID(),
        [Title] NVARCHAR(255) NOT NULL,
        [Slug] NVARCHAR(255) NOT NULL UNIQUE,
        [Department] NVARCHAR(100) NOT NULL,
        [ExperienceYears] INT NOT NULL,
        [JobType] NVARCHAR(50) NOT NULL,
        [ContractType] NVARCHAR(50) NOT NULL,
        [Location] NVARCHAR(255) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [ResponsibilitiesJson] NVARCHAR(MAX) NOT NULL,
        [QualificationsJson] NVARCHAR(MAX) NOT NULL,
        [SkillsJson] NVARCHAR(MAX) NOT NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'draft',
        [ApplicationDeadline] DATETIME2 NOT NULL,
        [IsFeatured] BIT NOT NULL DEFAULT 0,
        [BannerImageUrl] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- JobApplications Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JobApplications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[JobApplications] (
        [Id] Guid PRIMARY KEY DEFAULT NEWID(),
        [JobId] Guid NOT NULL,
        [JobTitle] NVARCHAR(255) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(255) NOT NULL,
        [Phone] NVARCHAR(20) NOT NULL,
        [Address] NVARCHAR(500) NOT NULL,
        [ExperienceJson] NVARCHAR(MAX) NOT NULL,
        [EducationJson] NVARCHAR(MAX) NOT NULL,
        [ResumeUrl] NVARCHAR(500) NOT NULL,
        [CoverMessage] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'new',
        [Notes] NVARCHAR(MAX) NULL,
        [AppliedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_JobApplications_JobPostings] FOREIGN KEY ([JobId]) REFERENCES [dbo].[JobPostings]([Id]) ON DELETE CASCADE
    );
END
GO

-- Stored Procedures

-- Create Job
CREATE OR ALTER PROCEDURE sp_CreateJob
    @Title NVARCHAR(255),
    @Slug NVARCHAR(255),
    @Department NVARCHAR(100),
    @ExperienceYears INT,
    @JobType NVARCHAR(50),
    @ContractType NVARCHAR(50),
    @Location NVARCHAR(255),
    @Description NVARCHAR(MAX),
    @ResponsibilitiesJson NVARCHAR(MAX),
    @QualificationsJson NVARCHAR(MAX),
    @SkillsJson NVARCHAR(MAX),
    @Status NVARCHAR(20),
    @ApplicationDeadline DATETIME2,
    @IsFeatured BIT,
    @BannerImageUrl NVARCHAR(500),
    @NewJobId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET @NewJobId = NEWID();
    INSERT INTO [dbo].[JobPostings] (Id, Title, Slug, Department, ExperienceYears, JobType, ContractType, Location, Description, ResponsibilitiesJson, QualificationsJson, SkillsJson, Status, ApplicationDeadline, IsFeatured, BannerImageUrl)
    VALUES (@NewJobId, @Title, @Slug, @Department, @ExperienceYears, @JobType, @ContractType, @Location, @Description, @ResponsibilitiesJson, @QualificationsJson, @SkillsJson, @Status, @ApplicationDeadline, @IsFeatured, @BannerImageUrl);
END
GO

-- Update Job
CREATE OR ALTER PROCEDURE sp_UpdateJob
    @Id UNIQUEIDENTIFIER,
    @Title NVARCHAR(255),
    @Slug NVARCHAR(255),
    @Department NVARCHAR(100),
    @ExperienceYears INT,
    @JobType NVARCHAR(50),
    @ContractType NVARCHAR(50),
    @Location NVARCHAR(255),
    @Description NVARCHAR(MAX),
    @ResponsibilitiesJson NVARCHAR(MAX),
    @QualificationsJson NVARCHAR(MAX),
    @SkillsJson NVARCHAR(MAX),
    @Status NVARCHAR(20),
    @ApplicationDeadline DATETIME2,
    @IsFeatured BIT,
    @BannerImageUrl NVARCHAR(500)
AS
BEGIN
    UPDATE [dbo].[JobPostings]
    SET Title = @Title,
        Slug = @Slug,
        Department = @Department,
        ExperienceYears = @ExperienceYears,
        JobType = @JobType,
        ContractType = @ContractType,
        Location = @Location,
        Description = @Description,
        ResponsibilitiesJson = @ResponsibilitiesJson,
        QualificationsJson = @QualificationsJson,
        SkillsJson = @SkillsJson,
        Status = @Status,
        ApplicationDeadline = @ApplicationDeadline,
        IsFeatured = @IsFeatured,
        BannerImageUrl = @BannerImageUrl,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END
GO

-- Delete Job
CREATE OR ALTER PROCEDURE sp_DeleteJob
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    DELETE FROM [dbo].[JobPostings] WHERE Id = @Id;
END
GO

-- Get Jobs
CREATE OR ALTER PROCEDURE sp_GetJobs
    @Status NVARCHAR(20) = NULL,
    @Department NVARCHAR(100) = NULL,
    @Location NVARCHAR(255) = NULL
AS
BEGIN
    SELECT * FROM [dbo].[JobPostings]
    WHERE (@Status IS NULL OR Status = @Status)
      AND (@Department IS NULL OR Department = @Department)
      AND (@Location IS NULL OR Location = @Location)
    ORDER BY CreatedAt DESC;
END
GO

-- Get Job By Id
CREATE OR ALTER PROCEDURE sp_GetJobById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT * FROM [dbo].[JobPostings] WHERE Id = @Id;
END
GO

-- Get Job By Slug
CREATE OR ALTER PROCEDURE sp_GetJobBySlug
    @Slug NVARCHAR(255)
AS
BEGIN
    SELECT * FROM [dbo].[JobPostings] WHERE Slug = @Slug;
END
GO

-- Create Application
CREATE OR ALTER PROCEDURE sp_CreateApplication
    @JobId UNIQUEIDENTIFIER,
    @JobTitle NVARCHAR(255),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(255),
    @Phone NVARCHAR(20),
    @Address NVARCHAR(500),
    @ExperienceJson NVARCHAR(MAX),
    @EducationJson NVARCHAR(MAX),
    @ResumeUrl NVARCHAR(500),
    @CoverMessage NVARCHAR(MAX),
    @NewApplicationId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET @NewApplicationId = NEWID();
    INSERT INTO [dbo].[JobApplications] (Id, JobId, JobTitle, FirstName, LastName, Email, Phone, Address, ExperienceJson, EducationJson, ResumeUrl, CoverMessage)
    VALUES (@NewApplicationId, @JobId, @JobTitle, @FirstName, @LastName, @Email, @Phone, @Address, @ExperienceJson, @EducationJson, @ResumeUrl, @CoverMessage);
END
GO

-- Update Application Status
CREATE OR ALTER PROCEDURE sp_UpdateApplicationStatus
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @Notes NVARCHAR(MAX)
AS
BEGIN
    UPDATE [dbo].[JobApplications]
    SET Status = @Status,
        Notes = @Notes
    WHERE Id = @Id;
END
GO

-- Delete Application
CREATE OR ALTER PROCEDURE sp_DeleteApplication
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    DELETE FROM [dbo].[JobApplications] WHERE Id = @Id;
END
GO

-- Get Applications
CREATE OR ALTER PROCEDURE sp_GetApplications
    @JobId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(20) = NULL,
    @Search NVARCHAR(255) = NULL
AS
BEGIN
    SELECT * FROM [dbo].[JobApplications]
    WHERE (@JobId IS NULL OR JobId = @JobId)
      AND (@Status IS NULL OR Status = @Status)
      AND (@Search IS NULL OR FirstName LIKE '%' + @Search + '%' OR LastName LIKE '%' + @Search + '%' OR Email LIKE '%' + @Search + '%')
    ORDER BY AppliedAt DESC;
END
GO

-- Get Application By Id
CREATE OR ALTER PROCEDURE sp_GetApplicationById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT * FROM [dbo].[JobApplications] WHERE Id = @Id;
END
GO
