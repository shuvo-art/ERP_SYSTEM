using Dapper;
using Microsoft.Data.SqlClient;
using Auth.Core.Entities;
using Auth.Core.Interfaces;
using System.Data;

namespace Auth.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly string _connectionString;

    public AuthRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<User>(
            "sp_GetUserByEmail",
            new { Email = email },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<User>(
            "sp_GetUserById",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> CreateUserAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Email", user.Email);
        parameters.Add("@PasswordHash", user.PasswordHash);
        parameters.Add("@FirstName", user.FirstName);
        parameters.Add("@LastName", user.LastName);
        parameters.Add("@Role", user.Role);
        parameters.Add("@Phone", user.Phone);
        parameters.Add("@Country", user.Country);
        parameters.Add("@NewUserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreateUser", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("@NewUserId");
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(
            "sp_UpdateUser",
            new { 
                Id = user.Id, 
                FirstName = user.FirstName, 
                LastName = user.LastName, 
                Phone = user.Phone, 
                Country = user.Country,
                Language = user.Language,
                ProfileImage = user.ProfileImage
            },
            commandType: CommandType.StoredProcedure
        );
        return rows > 0;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(
            "UPDATE Users SET IsActive = 0 WHERE Id = @UserId",
            new { UserId = userId }
        );
        return rows > 0;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<User>("sp_GetAllUsers", commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("sp_UpdateLastLogin", new { UserId = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task RecordFailedLoginAsync(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        // Using direct SQL for simplicity if sp not found or to keep current logic
        await connection.ExecuteAsync("sp_RecordFailedLogin", new { Email = email }, commandType: CommandType.StoredProcedure);
    }

    public async Task ResetFailedLoginAttemptsAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("sp_ResetFailedLoginAttempts", new { UserId = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", token.UserId);
        parameters.Add("@Token", token.Token);
        parameters.Add("@ExpiresAt", token.ExpiresAt);
        parameters.Add("@NewTokenId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreateRefreshToken", parameters, commandType: CommandType.StoredProcedure);
        token.Id = parameters.Get<int>("@NewTokenId");
        return token;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<RefreshToken>(
            "sp_ValidateRefreshToken",
            new { Token = token },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("sp_RevokeRefreshToken", new { Token = token }, commandType: CommandType.StoredProcedure);
    }

    public async Task RevokeAllUserTokensAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("sp_RevokeAllUserTokens", new { UserId = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<PasswordResetToken> CreatePasswordResetTokenAsync(PasswordResetToken token)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", token.UserId);
        parameters.Add("@Token", token.Token);
        parameters.Add("@ExpiresAt", token.ExpiresAt);
        parameters.Add("@NewTokenId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await connection.ExecuteAsync("sp_CreatePasswordResetToken", parameters, commandType: CommandType.StoredProcedure);
        token.Id = parameters.Get<int>("@NewTokenId");
        return token;
    }

    public async Task<PasswordResetToken?> ValidatePasswordResetTokenAsync(string token)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<PasswordResetToken>(
            "sp_ValidatePasswordResetToken",
            new { Token = token },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task UpdatePasswordAsync(int userId, string passwordHash, string? resetToken = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "sp_UpdatePassword",
            new { UserId = userId, PasswordHash = passwordHash },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> SetEmailVerificationOTPAsync(int userId, string otp, DateTime expiry)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(
            "sp_SetEmailVerificationOTP",
            new { UserId = userId, OTP = otp, Expiry = expiry },
            commandType: CommandType.StoredProcedure
        );
        return rows > 0;
    }

    public async Task<User?> VerifyEmailOTPAsync(string email, string otp)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<User>(
            "sp_VerifyEmailOTP",
            new { Email = email, OTP = otp },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> SetPasswordResetOTPAsync(int userId, string otp, DateTime expiry)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(
            "sp_SetPasswordResetOTP",
            new { UserId = userId, OTP = otp, Expiry = expiry },
            commandType: CommandType.StoredProcedure
        );
        return rows > 0;
    }

    public async Task<bool> VerifyPasswordResetOTPAsync(string email, string otp)
    {
        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<int>(
            "sp_VerifyPasswordResetOTP",
            new { Email = email, OTP = otp },
            commandType: CommandType.StoredProcedure
        );
        return result == 1;
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, string role)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(
            "sp_UpdateUserRole",
            new { UserId = userId, Role = role },
            commandType: CommandType.StoredProcedure
        );
        return rows > 0;
    }

    public async Task<object> GetUserStatisticsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<object>("sp_GetUserStatistics", commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateFcmTokenAsync(int userId, string token)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(
            "sp_UpdateFcmToken",
            new { UserId = userId, Token = token },
            commandType: CommandType.StoredProcedure
        );
        return rows > 0;
    }

    public async Task LogAuditEventAsync(AuditLog auditLog)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "sp_LogAuditEvent",
            new { 
                UserId = auditLog.UserId, 
                Action = auditLog.Action, 
                IpAddress = auditLog.IpAddress, 
                UserAgent = auditLog.UserAgent, 
                Details = auditLog.Details, 
                Success = auditLog.Success 
            },
            commandType: CommandType.StoredProcedure
        );
    }
}
