IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Master')
BEGIN
    CREATE DATABASE Master;
END
GO

USE Master;
GO

-- Enquiries Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Enquiries]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Enquiries] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [Type] NVARCHAR(50) NOT NULL, -- product_enquiry | partnership | dealership | technical_meeting | general
        [Name] NVARCHAR(255) NOT NULL,
        [Designation] NVARCHAR(255) NULL,
        [Mobile] NVARCHAR(20) NOT NULL,
        [Email] NVARCHAR(255) NOT NULL,
        [Address] NVARCHAR(500) NULL,
        [Country] NVARCHAR(100) NULL,
        [CompanyName] NVARCHAR(255) NULL,
        [ProductId] NVARCHAR(255) NULL,
        [Message] NVARCHAR(MAX) NOT NULL,
        [RequestCallBack] BIT NOT NULL DEFAULT 0,
        [AgreeDataProtection] BIT NOT NULL DEFAULT 0,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'new', -- new | responded | closed
        [AdminNotes] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Distributors Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Distributors]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Distributors] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [Name] NVARCHAR(255) NOT NULL,
        [Address] NVARCHAR(500) NOT NULL,
        [Phone] NVARCHAR(50) NOT NULL,
        [Country] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(255) NULL,
        [Website] NVARCHAR(255) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Stored Procedures

-- Create Enquiry
CREATE OR ALTER PROCEDURE sp_CreateEnquiry
    @Type NVARCHAR(50),
    @Name NVARCHAR(255),
    @Designation NVARCHAR(255),
    @Mobile NVARCHAR(20),
    @Email NVARCHAR(255),
    @Address NVARCHAR(500),
    @Country NVARCHAR(100),
    @CompanyName NVARCHAR(255),
    @ProductId NVARCHAR(255),
    @Message NVARCHAR(MAX),
    @RequestCallBack BIT,
    @AgreeDataProtection BIT,
    @NewEnquiryId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET @NewEnquiryId = NEWID();
    INSERT INTO [dbo].[Enquiries] (Id, Type, Name, Designation, Mobile, Email, Address, Country, CompanyName, ProductId, Message, RequestCallBack, AgreeDataProtection)
    VALUES (@NewEnquiryId, @Type, @Name, @Designation, @Mobile, @Email, @Address, @Country, @CompanyName, @ProductId, @Message, @RequestCallBack, @AgreeDataProtection);
END
GO

-- Get Enquiries
CREATE OR ALTER PROCEDURE sp_GetEnquiries
    @Type NVARCHAR(50) = NULL,
    @Status NVARCHAR(20) = NULL,
    @Search NVARCHAR(255) = NULL,
    @DateFrom DATETIME2 = NULL,
    @DateTo DATETIME2 = NULL
AS
BEGIN
    SELECT * FROM [dbo].[Enquiries]
    WHERE (@Type IS NULL OR Type = @Type)
      AND (@Status IS NULL OR Status = @Status)
      AND (@Search IS NULL OR Name LIKE '%' + @Search + '%' OR Email LIKE '%' + @Search + '%' OR CompanyName LIKE '%' + @Search + '%')
      AND (@DateFrom IS NULL OR CreatedAt >= @DateFrom)
      AND (@DateTo IS NULL OR CreatedAt <= @DateTo)
    ORDER BY CreatedAt DESC;
END
GO

-- Get Enquiry By Id
CREATE OR ALTER PROCEDURE sp_GetEnquiryById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT * FROM [dbo].[Enquiries] WHERE Id = @Id;
END
GO

-- Update Enquiry Status/Notes
CREATE OR ALTER PROCEDURE sp_UpdateEnquiry
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @AdminNotes NVARCHAR(MAX)
AS
BEGIN
    UPDATE [dbo].[Enquiries]
    SET Status = @Status,
        AdminNotes = @AdminNotes,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END
GO

-- Create Distributor
CREATE OR ALTER PROCEDURE sp_CreateDistributor
    @Name NVARCHAR(255),
    @Address NVARCHAR(500),
    @Phone NVARCHAR(50),
    @Country NVARCHAR(100),
    @Email NVARCHAR(255),
    @Website NVARCHAR(255),
    @IsActive BIT,
    @DisplayOrder INT,
    @NewDistributorId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET @NewDistributorId = NEWID();
    INSERT INTO [dbo].[Distributors] (Id, Name, Address, Phone, Country, Email, Website, IsActive, DisplayOrder)
    VALUES (@NewDistributorId, @Name, @Address, @Phone, @Country, @Email, @Website, @IsActive, @DisplayOrder);
END
GO

-- Get Distributors
CREATE OR ALTER PROCEDURE sp_GetDistributors
    @IsActive BIT = NULL
AS
BEGIN
    SELECT * FROM [dbo].[Distributors]
    WHERE (@IsActive IS NULL OR IsActive = @IsActive)
    ORDER BY DisplayOrder ASC, Name ASC;
END
GO

-- Get Distributor By Id
CREATE OR ALTER PROCEDURE sp_GetDistributorById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT * FROM [dbo].[Distributors] WHERE Id = @Id;
END
GO

-- Update Distributor
CREATE OR ALTER PROCEDURE sp_UpdateDistributor
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(255),
    @Address NVARCHAR(500),
    @Phone NVARCHAR(50),
    @Country NVARCHAR(100),
    @Email NVARCHAR(255),
    @Website NVARCHAR(255),
    @IsActive BIT,
    @DisplayOrder INT
AS
BEGIN
    UPDATE [dbo].[Distributors]
    SET Name = @Name,
        Address = @Address,
        Phone = @Phone,
        Country = @Country,
        Email = @Email,
        Website = @Website,
        IsActive = @IsActive,
        DisplayOrder = @DisplayOrder,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END
GO

-- Delete Distributor
CREATE OR ALTER PROCEDURE sp_DeleteDistributor
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    DELETE FROM [dbo].[Distributors] WHERE Id = @Id;
END
GO

-- Reorder Distributors
-- Note: This is a bit tricky with stored procedures if we want to do bulk update.
-- For simplicity, we can call sp_UpdateDistributor for each item or create a custom procedure if needed.
-- But since the request mentions a PATCH /api/v1/distributors/reorder with a list of IDs, 
-- we might just handle the loop in the service/repo.
