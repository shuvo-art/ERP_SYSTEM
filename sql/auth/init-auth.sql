-- ============================================
-- Auth Module - Database Schema
-- ============================================

-- Users Table with Account Lockout & Advanced Profile
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(50) NULL,
        LastName NVARCHAR(50) NULL,
        Role NVARCHAR(20) NOT NULL DEFAULT 'User',
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2 NULL,
        FailedLoginAttempts INT NOT NULL DEFAULT 0,
        LockoutEnd DATETIME2 NULL,
        
        -- Advanced Profile Properties
        Phone NVARCHAR(20) NULL,
        Country NVARCHAR(100) NULL,
        ProfileImage NVARCHAR(500) NULL,
        Language NVARCHAR(10) NOT NULL DEFAULT 'en',
        FcmToken NVARCHAR(500) NULL,
        
        -- Verification Properties
        IsEmailVerified BIT NOT NULL DEFAULT 0,
        EmailVerificationOTP NVARCHAR(10) NULL,
        EmailVerificationExpires DATETIME2 NULL,
        PasswordResetOTP NVARCHAR(10) NULL,
        PasswordResetExpires DATETIME2 NULL,
        Status INT NOT NULL DEFAULT 0 -- 0: Pending, 1: Active, 2: Suspended, 3: Deactivated
    );
    
    CREATE INDEX IX_Users_Email ON Users(Email);
    
    -- Seed admin user (password: Admin123!)
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, IsEmailVerified, Status)
    VALUES ('admin@erpsystem.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu3u6u', 'Admin', 'User', 'Admin', 1, 1);
END
GO

-- RefreshTokens Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
        Token NVARCHAR(255) NOT NULL UNIQUE,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        RevokedAt DATETIME2 NULL,
        IsRevoked BIT NOT NULL DEFAULT 0
    );
    
    CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
END
GO

-- AuditLogs Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
        Action NVARCHAR(100) NOT NULL,
        IpAddress NVARCHAR(45) NULL,
        UserAgent NVARCHAR(500) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        Details NVARCHAR(MAX) NULL,
        Success BIT NOT NULL DEFAULT 1
    );
    
    CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
    CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
    CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
END
GO

-- ============================================
-- Auth Module - Stored Procedures
-- ============================================

-- Create User (Registration)
CREATE OR ALTER PROCEDURE sp_CreateUser
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Role NVARCHAR(20) = 'User',
    @Phone NVARCHAR(20) = NULL,
    @Country NVARCHAR(100) = NULL,
    @NewUserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        SET @NewUserId = -1;
        RETURN;
    END
    
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, Phone, Country, Status)
    VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Role, @Phone, @Country, 0);
    
    SET @NewUserId = SCOPE_IDENTITY();
END
GO

-- Get User by Email
CREATE OR ALTER PROCEDURE sp_GetUserByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Users WHERE Email = @Email AND IsActive = 1;
END
GO

-- Get User by ID
CREATE OR ALTER PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Users WHERE Id = @UserId AND IsActive = 1;
END
GO

-- Update Last Login
CREATE OR ALTER PROCEDURE sp_UpdateLastLogin
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET LastLoginAt = GETUTCDATE() WHERE Id = @UserId;
END
GO

-- OTP Verification Procedures
CREATE OR ALTER PROCEDURE sp_SetEmailVerificationOTP
    @UserId INT,
    @OTP NVARCHAR(10),
    @Expiry DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET 
        EmailVerificationOTP = @OTP,
        EmailVerificationExpires = @Expiry
    WHERE Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE sp_VerifyEmailOTP
    @Email NVARCHAR(100),
    @OTP NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET 
        IsEmailVerified = 1,
        Status = 1, -- Active
        EmailVerificationOTP = NULL,
        EmailVerificationExpires = NULL
    WHERE Email = @Email 
      AND EmailVerificationOTP = @OTP 
      AND EmailVerificationExpires > GETUTCDATE();
      
    SELECT * FROM Users WHERE Email = @Email AND IsEmailVerified = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_SetPasswordResetOTP
    @UserId INT,
    @OTP NVARCHAR(10),
    @Expiry DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET 
        PasswordResetOTP = @OTP,
        PasswordResetExpires = @Expiry
    WHERE Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE sp_VerifyPasswordResetOTP
    @Email NVARCHAR(100),
    @OTP NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND PasswordResetOTP = @OTP AND PasswordResetExpires > GETUTCDATE())
    BEGIN
        SELECT 1;
    END
    ELSE
    BEGIN
        SELECT 0;
    END
END
GO

-- Profile Management
CREATE OR ALTER PROCEDURE sp_UpdateUser
    @Id INT,
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Phone NVARCHAR(20) = NULL,
    @Country NVARCHAR(100) = NULL,
    @Language NVARCHAR(10) = 'en',
    @ProfileImage NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET 
        FirstName = @FirstName,
        LastName = @LastName,
        Phone = @Phone,
        Country = @Country,
        Language = @Language,
        ProfileImage = @ProfileImage
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdatePassword
    @UserId INT,
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET 
        PasswordHash = @PasswordHash,
        PasswordResetOTP = NULL,
        PasswordResetExpires = NULL
    WHERE Id = @UserId;
END
GO

-- Admin Procedures
CREATE OR ALTER PROCEDURE sp_GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Users WHERE IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateUserRole
    @UserId INT,
    @Role NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET Role = @Role WHERE Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetUserStatistics
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        (SELECT COUNT(*) FROM Users WHERE IsActive = 1) AS TotalUsers,
        (SELECT COUNT(*) FROM Users WHERE Role = 'Admin' AND IsActive = 1) AS AdminUsers,
        (SELECT COUNT(*) FROM Users WHERE Status = 1 AND IsActive = 1) AS ActiveUsers,
        (SELECT COUNT(*) FROM Users WHERE CreatedAt >= DATEADD(day, -30, GETUTCDATE())) AS NewUsersLast30Days;
END
GO

-- Audit & FCM
CREATE OR ALTER PROCEDURE sp_UpdateFcmToken
    @UserId INT,
    @Token NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET FcmToken = @Token WHERE Id = @UserId;
END
GO

-- Log Audit Event
CREATE OR ALTER PROCEDURE sp_LogAuditEvent
    @UserId INT = NULL,
    @Action NVARCHAR(100),
    @IpAddress NVARCHAR(45) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @Details NVARCHAR(MAX) = NULL,
    @Success BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AuditLogs (UserId, Action, IpAddress, UserAgent, Details, Success)
    VALUES (@UserId, @Action, @IpAddress, @UserAgent, @Details, @Success);
END
GO

-- Account Lockout Procedures
CREATE OR ALTER PROCEDURE sp_RecordFailedLogin
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MaxAttempts INT = 5;
    DECLARE @LockoutMinutes INT = 15;
    
    UPDATE Users
    SET FailedLoginAttempts = FailedLoginAttempts + 1,
        LockoutEnd = CASE 
            WHEN FailedLoginAttempts + 1 >= @MaxAttempts 
            THEN DATEADD(MINUTE, @LockoutMinutes, GETUTCDATE())
            ELSE NULL
        END
    WHERE Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE sp_ResetFailedLoginAttempts
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users
    SET FailedLoginAttempts = 0,
        LockoutEnd = NULL
    WHERE Id = @UserId;
END
GO

-- Refresh Token Procedures
CREATE OR ALTER PROCEDURE sp_CreateRefreshToken
    @UserId INT,
    @Token NVARCHAR(255),
    @ExpiresAt DATETIME2,
    @NewTokenId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO RefreshTokens (UserId, Token, ExpiresAt)
    VALUES (@UserId, @Token, @ExpiresAt);
    SET @NewTokenId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_ValidateRefreshToken
    @Token NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT rt.*, u.Email, u.FirstName, u.LastName, u.Role
    FROM RefreshTokens rt
    INNER JOIN Users u ON rt.UserId = u.Id
    WHERE rt.Token = @Token 
      AND rt.IsRevoked = 0 
      AND rt.ExpiresAt > GETUTCDATE()
      AND u.IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_RevokeRefreshToken
    @Token NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE RefreshTokens SET IsRevoked = 1, RevokedAt = GETUTCDATE() WHERE Token = @Token;
END
GO

CREATE OR ALTER PROCEDURE sp_RevokeAllUserTokens
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE RefreshTokens SET IsRevoked = 1, RevokedAt = GETUTCDATE() WHERE UserId = @UserId AND IsRevoked = 0;
END
GO
