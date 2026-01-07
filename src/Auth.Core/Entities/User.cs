using Auth.Core.Enums;

namespace Auth.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Account Lockout Properties
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    // Advanced Profile Properties
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? ProfileImage { get; set; }
    public string Language { get; set; } = "en";
    public string? FcmToken { get; set; }

    // Verification Properties
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationOTP { get; set; }
    public DateTime? EmailVerificationExpires { get; set; }
    public string? PasswordResetOTP { get; set; }
    public DateTime? PasswordResetExpires { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Pending;
}
