using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth.Core.Entities;
using Auth.Core.Interfaces;
using Auth.Core.Enums;
using System.Security.Claims;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserController> logger)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var user = await _authRepository.GetUserByIdAsync(int.Parse(userIdClaim.Value));
        if (user == null) return NotFound(new { message = "User not found" });

        return Ok(new { 
            user.Id, 
            user.Email, 
            user.FirstName, 
            user.LastName, 
            user.Role, 
            user.Phone, 
            user.Country, 
            user.ProfileImage, 
            user.Language,
            user.Status,
            user.IsEmailVerified,
            user.CreatedAt,
            user.LastLoginAt
        });
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] User updateRequest)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);
        var user = await _authRepository.GetUserByIdAsync(userId);
        if (user == null) return NotFound(new { message = "User not found" });

        // Only allowed updates
        user.FirstName = updateRequest.FirstName ?? user.FirstName;
        user.LastName = updateRequest.LastName ?? user.LastName;
        user.Phone = updateRequest.Phone ?? user.Phone;
        user.Country = updateRequest.Country ?? user.Country;
        user.Language = updateRequest.Language ?? user.Language;
        // ProfileImage would normally be handled via separate upload endpoint, but allowing URL here for parity
        user.ProfileImage = updateRequest.ProfileImage ?? user.ProfileImage;

        var success = await _authRepository.UpdateUserAsync(user);
        if (!success) return StatusCode(500, new { message = "Failed to update profile" });

        return Ok(new { message = "Profile updated successfully" });
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);
        var user = await _authRepository.GetUserByIdAsync(userId);
        if (user == null) return NotFound(new { message = "User not found" });

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Incorrect current password" });
        }

        var newHash = _passwordHasher.HashPassword(request.NewPassword);
        await _authRepository.UpdatePasswordAsync(userId, newHash);

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Update FCM Token
    /// </summary>
    [HttpPost("fcm-token")]
    public async Task<IActionResult> UpdateFcmToken([FromBody] string token)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        await _authRepository.UpdateFcmTokenAsync(int.Parse(userIdClaim.Value), token);
        return Ok(new { message = "FCM token updated successfully" });
    }

    // Admin Routes

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authRepository.GetUsersAsync();
        return Ok(users.Select(u => new { 
            u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.Status, u.CreatedAt 
        }));
    }

    /// <summary>
    /// Get user statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _authRepository.GetUserStatisticsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Update user role (Admin only)
    /// </summary>
    [HttpPut("{userId}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRole(int userId, [FromBody] string role)
    {
        if (role != "User" && role != "Admin") return BadRequest(new { message = "Invalid role" });
        
        var success = await _authRepository.UpdateUserRoleAsync(userId, role);
        if (!success) return NotFound(new { message = "User not found" });

        return Ok(new { message = "User role updated successfully" });
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (userId == currentUserId) return BadRequest(new { message = "You cannot delete your own account" });

        var success = await _authRepository.DeleteUserAsync(userId);
        if (!success) return NotFound(new { message = "User not found" });

        return Ok(new { message = "User deleted successfully" });
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
