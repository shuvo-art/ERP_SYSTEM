using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth.Core.Entities;
using Auth.Core.Interfaces;
using Auth.Core.Enums;
using Auth.Core.DTOs;
using System.Security.Claims;
using Shared.Kernel.Interfaces;
using Shared.Kernel.Services;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserController> _logger;
    private readonly ICacheService _cacheService;
    private readonly IMessagePublisher _messagePublisher;

    public UserController(
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserController> logger,
        ICacheService cacheService,
        IMessagePublisher messagePublisher)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _cacheService = cacheService;
        _messagePublisher = messagePublisher;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);
        var cacheKey = $"user_profile_{userId}";

        // Try to get from Cache
        var user = await _cacheService.GetAsync<User>(cacheKey);
        if (user == null)
        {
            _logger.LogInformation("Cache miss for user profile: {UserId}. Fetching from DB.", userId);
            user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            // Save to Cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
        }

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
    /// Update current user profile — publishes Pub/Sub event for distributed cache invalidation (Task 3)
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
        user.ProfileImage = updateRequest.ProfileImage ?? user.ProfileImage;

        var success = await _authRepository.UpdateUserAsync(user);
        if (!success) return StatusCode(500, new { message = "Failed to update profile" });

        // ─── Task 3: Redis Pub/Sub — Distributed Cache Invalidation ───────────
        // 1. Directly remove from THIS instance's cache.
        await _cacheService.RemoveAsync($"user_profile_{userId}");

        // 2. Publish a message to the "user-updates" channel.
        //    ALL other running instances of the Auth service are subscribed to this channel
        //    via UserCacheInvalidationService and will also clear their copy of the cache.
        await _messagePublisher.PublishAsync(UserCacheInvalidationService.Channel, userId.ToString());
        // ──────────────────────────────────────────────────────────────────────

        _logger.LogInformation("Profile updated and cache invalidation published for UserId: {UserId}", userId);

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

    // ─── Admin Routes ─────────────────────────────────────────────────────────

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
    /// Update user role (Admin only) — publishes Pub/Sub event for distributed cache invalidation (Task 3)
    /// </summary>
    [HttpPut("{userId}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRole(int userId, [FromBody] UpdateRoleRequest request)
    {
        if (request.Role != "User" && request.Role != "Admin")
            return BadRequest(new { message = "Invalid role" });
        
        var success = await _authRepository.UpdateUserRoleAsync(userId, request.Role);
        if (!success) 
        {
            _logger.LogWarning("Failed to update role for user {UserId}. User not found or inactive.", userId);
            return NotFound(new { message = "User not found" });
        }

        // ─── Task 3: Redis Pub/Sub — Distributed Cache Invalidation ───────────
        // Role changes are security-critical — stale cache could grant wrong permissions.
        // Publishing to "user-updates" ensures ALL instances immediately invalidate
        // this user's cached profile, forcing a fresh DB read on next request.
        await _cacheService.RemoveAsync($"user_profile_{userId}");
        await _messagePublisher.PublishAsync(UserCacheInvalidationService.Channel, userId.ToString());
        // ──────────────────────────────────────────────────────────────────────

        _logger.LogInformation("Role updated to '{Role}' and cache invalidation published for UserId: {UserId}", request.Role, userId);

        return Ok(new { message = "User role updated successfully" });
    }

    /// <summary>
    /// Delete user (Admin only) — publishes Pub/Sub event for distributed cache invalidation (Task 3)
    /// </summary>
    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (userId == currentUserId) return BadRequest(new { message = "You cannot delete your own account" });

        var success = await _authRepository.DeleteUserAsync(userId);
        if (!success) return NotFound(new { message = "User not found" });

        // ─── Task 3: Redis Pub/Sub — Distributed Cache Invalidation ───────────
        // Even though the user is deleted, we should remove their stale cache entry
        // from all instances to prevent serving 404-worthy data from cache.
        await _cacheService.RemoveAsync($"user_profile_{userId}");
        await _messagePublisher.PublishAsync(UserCacheInvalidationService.Channel, userId.ToString());
        // ──────────────────────────────────────────────────────────────────────

        _logger.LogInformation("User deleted and cache invalidation published for UserId: {UserId}", userId);

        return Ok(new { message = "User deleted successfully" });
    }
}
