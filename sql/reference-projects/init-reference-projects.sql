-- ============================================
-- Reference Projects Module - New Database Schema
-- ============================================

-- 1. Project Categories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProjectCategories')
BEGIN
    CREATE TABLE ProjectCategories (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        ImageUrl NVARCHAR(MAX) NULL,
        Slug NVARCHAR(255) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- 2. Reference Projects
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReferenceProjects')
BEGIN
    CREATE TABLE ReferenceProjects (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ProjectName NVARCHAR(500) NOT NULL,
        Slug NVARCHAR(255) NOT NULL UNIQUE,
        Location NVARCHAR(MAX) NULL,
        OwnerName NVARCHAR(MAX) NULL,
        Contractor NVARCHAR(MAX) NULL,
        EngineerName NVARCHAR(MAX) NULL,
        ClientName NVARCHAR(MAX) NULL,
        ShortDescription NVARCHAR(MAX) NULL,
        DetailsDescription NVARCHAR(MAX) NULL, -- Rich text
        Status NVARCHAR(50) NOT NULL DEFAULT 'ongoing',
        StartDate DATETIME2 NULL,
        CompletionDate DATETIME2 NULL,
        Featured BIT NOT NULL DEFAULT 0,
        HeroImageUrl NVARCHAR(MAX) NULL,
        ProjectOverviewJson NVARCHAR(MAX) NULL,
        CategoryId INT NOT NULL FOREIGN KEY REFERENCES ProjectCategories(Id) ON DELETE CASCADE,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    CREATE INDEX IX_ReferenceProjects_Slug ON ReferenceProjects(Slug);
    CREATE INDEX IX_ReferenceProjects_CategoryId ON ReferenceProjects(CategoryId);
END
ELSE
BEGIN
    -- Update existing table if necessary (Add/Modify columns)
    -- This is just for initial setup consistency
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ReferenceProjects') AND name = 'CategoryId')
    BEGIN
        -- If we are migrating from old schema, this might be tricky with data. 
        -- Assuming fresh start or user will handle data.
        ALTER TABLE ReferenceProjects ADD CategoryId INT NULL; 
        ALTER TABLE ReferenceProjects ADD DetailsDescription NVARCHAR(MAX) NULL;
        ALTER TABLE ReferenceProjects ADD OwnerName NVARCHAR(MAX) NULL;
        ALTER TABLE ReferenceProjects ADD Contractor NVARCHAR(MAX) NULL;
        ALTER TABLE ReferenceProjects ADD EngineerName NVARCHAR(MAX) NULL;
        ALTER TABLE ReferenceProjects ADD ClientName NVARCHAR(MAX) NULL;
    END
END
GO

-- 3. Gallery Images
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProjectGalleryImages')
BEGIN
    CREATE TABLE ProjectGalleryImages (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ProjectId INT NOT NULL FOREIGN KEY REFERENCES ReferenceProjects(Id) ON DELETE CASCADE,
        ImageUrl NVARCHAR(MAX) NOT NULL
    );
END
GO

-- 4. Detail Images
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProjectDetailImages')
BEGIN
    CREATE TABLE ProjectDetailImages (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ProjectId INT NOT NULL FOREIGN KEY REFERENCES ReferenceProjects(Id) ON DELETE CASCADE,
        ImageUrl NVARCHAR(MAX) NOT NULL
    );
END
GO

-- 5. Project Products Junction
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProjectProducts')
BEGIN
    CREATE TABLE ProjectProducts (
        ProjectId INT NOT NULL FOREIGN KEY REFERENCES ReferenceProjects(Id) ON DELETE CASCADE,
        ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE CASCADE,
        PRIMARY KEY (ProjectId, ProductId)
    );
END
GO
