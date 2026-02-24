using Auth.Core.Entities;

namespace Auth.Core.Interfaces;

public interface IAuthRepository
{
    // User Basic Operations
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    Task<int> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int userId);
    Task<IEnumerable<User>> GetUsersAsync();
    
    // Auth Logic
    Task UpdateLastLoginAsync(int userId);
    Task RecordFailedLoginAsync(string email);
    Task ResetFailedLoginAttemptsAsync(int userId);
    
    // Refresh Tokens
    Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task RevokeAllUserTokensAsync(int userId);

    // ─── Refresh Token Rotation ──────────────────────────────────────────────
    /// <summary>
    /// Gets a refresh token entity by its raw token value, including IsRevoked/Family info.
    /// Unlike ValidateRefreshTokenAsync, this returns even revoked tokens (for breach detection).
    /// </summary>
    Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token);

    /// <summary>
    /// Revokes ALL tokens in the same TokenFamily.
    /// Called on breach detection (reuse of a revoked token).
    /// </summary>
    Task RevokeTokenFamilyAsync(string tokenFamily);

    /// <summary>
    /// Atomically: revoke the old token (set IsRevoked, RevokedAt, ReplacedByToken)
    /// and create the new rotated token. Returns the new token entity.
    /// </summary>
    Task<RefreshToken> ReplaceRefreshTokenAsync(string oldToken, RefreshToken newToken);
    
    // Password Reset
    Task<PasswordResetToken> CreatePasswordResetTokenAsync(PasswordResetToken token);
    Task<PasswordResetToken?> ValidatePasswordResetTokenAsync(string token);
    Task UpdatePasswordAsync(int userId, string passwordHash, string? resetToken = null);
    
    // OTP & Verification
    Task<bool> SetEmailVerificationOTPAsync(int userId, string otp, DateTime expiry);
    Task<User?> VerifyEmailOTPAsync(string email, string otp);
    Task<bool> SetPasswordResetOTPAsync(int userId, string otp, DateTime expiry);
    Task<bool> VerifyPasswordResetOTPAsync(string email, string otp);
    
    // Admin & Stats
    Task<bool> UpdateUserRoleAsync(int userId, string role);
    Task<object> GetUserStatisticsAsync();
    Task<bool> UpdateFcmTokenAsync(int userId, string token);
    
    // Audit
    Task LogAuditEventAsync(AuditLog auditLog);
}
