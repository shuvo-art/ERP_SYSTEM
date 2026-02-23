using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth.Core.DTOs;
using Auth.Core.Entities;
using Auth.Core.Interfaces;
using FluentValidation;
using Shared.Kernel.Interfaces;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly IValidator<ResendOtpRequest> _resendOtpValidator;
    private readonly ICacheService _cacheService;

    // ─── Rate Limit Constants ─────────────────────────────────────────────────
    // These constants define the behaviour of atomic increment rate limiting (Task 1).
    // All counters use Redis keys with a TTL so they self-expire; no cron job needed.
    private const int MaxLoginFailures       = 5;   // Block after 5 failed login attempts
    private const int MaxResendOtpRequests   = 3;   // Block after 3 resend OTP requests in the window
    private const int MaxForgotPasswordReqs  = 3;   // Block after 3 forgot-password requests in the window
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(15);

    public AuthController(
        IAuthRepository authRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IEmailService emailService,
        IOtpService otpService,
        IValidator<ResendOtpRequest> resendOtpValidator,
        ICacheService cacheService)
    {
        _authRepository = authRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _emailService = emailService;
        _otpService = otpService;
        _resendOtpValidator = resendOtpValidator;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Validate with FluentValidation
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            // Check if user already exists
            var existingUser = await _authRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "User with this email already exists" });
            }

            // Hash password
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "User"
            };

            var userId = await _authRepository.CreateUserAsync(user);

            if (userId == -1)
            {
                return Conflict(new { message = "User already exists" });
            }

            // Generate OTP
            var otp = _otpService.GenerateOtp();
            var expiry = DateTime.UtcNow.AddMinutes(15);
            
            await _authRepository.SetEmailVerificationOTPAsync(userId, otp, expiry);

            // Cache OTP in Redis for fast verification
            var cacheKey = $"otp_verify_{request.Email}";
            await _cacheService.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(15));

            // Send Email
            await _emailService.SendEmailAsync(
                request.Email,
                "Verify Your Email",
                $"Your verification code is: <strong>{otp}</strong>. It expires in 15 minutes."
            );

            // Audit log
            await _authRepository.LogAuditEventAsync(new AuditLog
            {
                UserId = userId,
                Action = "USER_REGISTERED_OTP_SENT",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Success = true
            });

            _logger.LogInformation("User registered and OTP sent: {Email}", request.Email);

            return Ok(new 
            {
                UserId = userId,
                Email = request.Email,
                Message = "Registration successful! Please check your email for the verification OTP."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { message = "Internal server error during registration" });
        }
    }

    /// <summary>
    /// Verify email with OTP — uses Lua atomic verify-and-delete (Task 2)
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpRequest request)
    {
        try
        {
            var cacheKey = $"otp_verify_{request.Email}";

            // ─── Task 2: Lua Atomic Verify & Delete ────────────────────────────────
            // AtomicVerifyAndDeleteAsync executes a Lua script on the Redis server that
            // checks AND deletes the OTP in one atomic step (no race condition possible).
            // Returns true only if the OTP existed and matched; deletes it either way.
            var otpConsumedFromCache = await _cacheService.AtomicVerifyAndDeleteAsync(cacheKey, request.Otp);

            if (!otpConsumedFromCache)
            {
                // OTP was not in cache (expired or never set) — fall back to DB verification (slow path).
                var user = await _authRepository.VerifyEmailOTPAsync(request.Email, request.Otp);
                if (user == null)
                {
                    _logger.LogWarning("Invalid or expired OTP attempt for email: {Email}", request.Email);
                    return BadRequest(new { message = "Invalid or expired OTP" });
                }

                // OTP was valid in DB; continue with the verified user.
                await CompleteEmailVerificationAsync(user);
                return await BuildEmailVerificationResponseAsync(user);
            }

            // OTP was valid and atomically consumed from cache (fast path).
            // We still need to confirm in the DB and mark the user as verified.
            var verifiedUser = await _authRepository.VerifyEmailOTPAsync(request.Email, request.Otp);
            if (verifiedUser == null)
            {
                // Extremely rare: cache had it but DB didn't (race during OTP resend).
                return BadRequest(new { message = "Invalid or expired OTP" });
            }

            await CompleteEmailVerificationAsync(verifiedUser);
            return await BuildEmailVerificationResponseAsync(verifiedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return StatusCode(500, new { message = "Internal server error during verification" });
        }
    }

    /// <summary>
    /// Resend verification OTP — uses atomic increment rate limiting (Task 1)
    /// </summary>
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
    {
        try
        {
            var validationResult = await _resendOtpValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            // ─── Task 1: Atomic Rate Limiting ──────────────────────────────────────
            // Check rate limit BEFORE touching the database to avoid unnecessary DB load.
            var rateLimitKey = $"resend_otp_rate:{request.Email}";
            var requestCount = await _cacheService.IncrementAsync(rateLimitKey, RateLimitWindow);

            if (requestCount > MaxResendOtpRequests)
            {
                _logger.LogWarning("ResendOtp rate limit exceeded for email: {Email} (count: {Count})", request.Email, requestCount);
                return StatusCode(429, new { message = $"Too many OTP requests. Please wait {(int)RateLimitWindow.TotalMinutes} minutes before trying again." });
            }
            // ──────────────────────────────────────────────────────────────────────

            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            
            // Industry standard: Don't reveal if user exists or not
            if (user == null)
            {
                return Ok(new { message = "If the account exists and is not verified, a new OTP has been sent." });
            }

            if (user.IsEmailVerified)
            {
                return BadRequest(new { message = "Email is already verified." });
            }

            // Generate new OTP
            var otp = _otpService.GenerateOtp();
            var expiry = DateTime.UtcNow.AddMinutes(15);
            
            await _authRepository.SetEmailVerificationOTPAsync(user.Id, otp, expiry);

            var cacheKey = $"otp_verify_{user.Email}";
            await _cacheService.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(15));

            // Audit log
            await _authRepository.LogAuditEventAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "OTP_RESENT",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Success = true
            });

            // Send Email
            await _emailService.SendEmailAsync(
                user.Email,
                "Verify Your Email - New OTP",
                $"Your new verification code is: <strong>{otp}</strong>. It expires in 15 minutes."
            );

            _logger.LogInformation("OTP resent for user: {Email}", request.Email);

            return Ok(new { message = "A new verification OTP has been sent to your email." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resending OTP");
            return StatusCode(500, new { message = "Internal server error during OTP resend" });
        }
    }

    /// <summary>
    /// Login — uses atomic increment rate limiting to block after 5 failed attempts (Task 1)
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validate with FluentValidation
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            // ─── Task 1: Atomic Rate Limiting — Checked BEFORE hitting the database ──
            // This is the key advantage: we block attackers before they can hammer the DB.
            var rateLimitKey = $"login_fail:{request.Email}";
            var currentFailCount = await _cacheService.GetAsync<long?>(rateLimitKey) ?? 0;

            if (currentFailCount >= MaxLoginFailures)
            {
                _logger.LogWarning("Login blocked by Redis rate limit for email: {Email} (failures: {Count})", request.Email, currentFailCount);
                return StatusCode(429, new { message = $"Account temporarily locked due to too many failed attempts. Please try again in {(int)RateLimitWindow.TotalMinutes} minutes." });
            }
            // ──────────────────────────────────────────────────────────────────────────

            // Get user by email
            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                // Increment the failure counter even for non-existent users to prevent user enumeration timing attacks.
                await _cacheService.IncrementAsync(rateLimitKey, RateLimitWindow);

                await _authRepository.LogAuditEventAsync(new AuditLog
                {
                    Action = "LOGIN_FAILED",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Details = $"User not found: {request.Email}",
                    Success = false
                });
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check if account is locked (DB-level lockout as a second layer of defence)
            if (user.IsLockedOut)
            {
                await _authRepository.LogAuditEventAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "LOGIN_BLOCKED_LOCKOUT",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Details = $"Account locked until: {user.LockoutEnd}",
                    Success = false
                });
                return Unauthorized(new { message = $"Account is locked. Please try again after {user.LockoutEnd:yyyy-MM-dd HH:mm:ss} UTC" });
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                // ─── Task 1: Increment failure counter on wrong password ──────────────
                var failCount = await _cacheService.IncrementAsync(rateLimitKey, RateLimitWindow);
                var remaining = MaxLoginFailures - (int)failCount;

                await _authRepository.RecordFailedLoginAsync(request.Email);
                await _authRepository.LogAuditEventAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "LOGIN_FAILED",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Details = $"Invalid password. Redis failure count: {failCount}/{MaxLoginFailures}",
                    Success = false
                });

                if (remaining <= 0)
                {
                    _logger.LogWarning("Account locked by Redis rate limit for email: {Email}", request.Email);
                    return StatusCode(429, new { message = $"Account temporarily locked due to too many failed attempts. Please try again in {(int)RateLimitWindow.TotalMinutes} minutes." });
                }

                return Unauthorized(new { message = $"Invalid email or password. {remaining} attempt(s) remaining before lockout." });
            }

            // Check if email is verified
            if (!user.IsEmailVerified)
            {
                // Industry standard: Cooldown check before resending
                bool otpSent = false;
                if (!user.EmailVerificationExpires.HasValue || 
                    user.EmailVerificationExpires.Value <= DateTime.UtcNow.AddMinutes(14))
                {
                    // Resend OTP
                    var otp = _otpService.GenerateOtp();
                    var expiry = DateTime.UtcNow.AddMinutes(15);
                    await _authRepository.SetEmailVerificationOTPAsync(user.Id, otp, expiry);

                    var cacheKey = $"otp_verify_{user.Email}";
                    await _cacheService.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(15));

                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Verify Your Email",
                        $"Your verification code is: <strong>{otp}</strong>. It expires in 15 minutes."
                    );
                    
                    otpSent = true;
                    _logger.LogInformation("OTP resent during login for user: {Email}", user.Email);
                }

                return StatusCode(403, new { 
                    message = otpSent ? "Please verify your email address. A new OTP has been sent." : "Please verify your email address. An OTP was recently sent to your email.",
                    email = user.Email
                });
            }

            // ─── Task 1: Clear rate limit key on successful login ─────────────────
            // This is important: once the user successfully logs in, we reset the counter
            // so they don't get locked out from their next session.
            await _cacheService.RemoveAsync(rateLimitKey);
            // ──────────────────────────────────────────────────────────────────────

            // Reset failed login attempts on successful login (DB-level)
            await _authRepository.ResetFailedLoginAttemptsAsync(user.Id);

            // Generate tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Store refresh token
            var refreshTokenExpirationDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.CreateRefreshTokenAsync(refreshTokenEntity);

            // Update last login
            await _authRepository.UpdateLastLoginAsync(user.Id);

            // Invalidate user profile cache to reflect new lastLoginAt
            await _cacheService.RemoveAsync($"user_profile_{user.Id}");

            // Audit log
            await _authRepository.LogAuditEventAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "LOGIN_SUCCESS",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Success = true
            });

            SetRefreshTokenCookie(refreshToken);

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return Ok(new
            {
                accessToken,
                refreshToken,
                userId = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            var storedToken = await _authRepository.ValidateRefreshTokenAsync(request.RefreshToken);
            if (storedToken == null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            var user = await _authRepository.GetUserByIdAsync(storedToken.UserId);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);

            _logger.LogInformation("Token refreshed for user: {Email}", user.Email);

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = request.RefreshToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Request password reset OTP — uses atomic increment rate limiting (Task 1)
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            // ─── Task 1: Atomic Rate Limiting (before DB lookup to prevent enumeration) ─
            var rateLimitKey = $"forgot_pwd_rate:{request.Email}";
            var requestCount = await _cacheService.IncrementAsync(rateLimitKey, RateLimitWindow);

            if (requestCount > MaxForgotPasswordReqs)
            {
                _logger.LogWarning("ForgotPassword rate limit exceeded for email: {Email} (count: {Count})", request.Email, requestCount);
                // Use the same generic message to avoid revealing if email exists
                return Ok(new { message = "If the email exists, a password reset OTP has been sent" });
            }
            // ──────────────────────────────────────────────────────────────────────

            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                // Return Ok to prevent user enumeration
                return Ok(new { message = "If the email exists, a password reset OTP has been sent" });
            }

            // Industry standard: Cooldown check
            if (user.PasswordResetExpires.HasValue && 
                user.PasswordResetExpires.Value > DateTime.UtcNow.AddMinutes(14))
            {
                return Ok(new { message = "If the email exists, a password reset OTP has been sent" });
            }

            var otp = _otpService.GenerateOtp();
            var expiry = DateTime.UtcNow.AddMinutes(15);
            
            await _authRepository.SetPasswordResetOTPAsync(user.Id, otp, expiry);

            var cacheKey = $"otp_pwd_reset_{user.Email}";
            await _cacheService.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(15));

            // Audit log
            await _authRepository.LogAuditEventAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "PASSWORD_RESET_REQUESTED",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Success = true
            });

            _logger.LogInformation("Password reset OTP requested for: {Email}", user.Email);
            
            // Send Email
            await _emailService.SendEmailAsync(
                user.Email,
                "Password Reset OTP",
                $"Your password reset code is: <strong>{otp}</strong>. It expires in 15 minutes."
            );

            return Ok(new { message = "If the email exists, a password reset OTP has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Reset password using OTP — uses Lua atomic verify-and-delete (Task 2)
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetVerifyRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Email, OTP and new password are required" });
            }

            var cacheKey = $"otp_pwd_reset_{request.Email}";

            // ─── Task 2: Lua Atomic Verify & Delete ────────────────────────────────
            // Atomically validates the OTP and deletes it in one Redis round-trip.
            // If two concurrent requests arrive at the exact same time, only ONE will
            // receive `true` — the other will get `false` because the key was already deleted.
            var otpConsumedFromCache = await _cacheService.AtomicVerifyAndDeleteAsync(cacheKey, request.Otp);

            if (!otpConsumedFromCache)
            {
                // Cache miss: OTP may have expired or was already used. Fall back to DB.
                var isValidFromDb = await _authRepository.VerifyPasswordResetOTPAsync(request.Email, request.Otp);
                if (!isValidFromDb)
                {
                    _logger.LogWarning("Invalid or expired password reset OTP for email: {Email}", request.Email);
                    return BadRequest(new { message = "Invalid or expired OTP" });
                }
            }
            // ──────────────────────────────────────────────────────────────────────

            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null) return NotFound(new { message = "User not found" });

            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            await _authRepository.UpdatePasswordAsync(user.Id, newPasswordHash);

            // Invalidate user profile cache
            await _cacheService.RemoveAsync($"user_profile_{user.Id}");

            // Audit log
            await _authRepository.LogAuditEventAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "PASSWORD_RESET_SUCCESS",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Success = true
            });

            _logger.LogInformation("Password reset successfully for user: {Email}", user.Email);

            return Ok(new { message = "Password reset successfully. Please login with your new password." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new { message = "Internal server error during password reset" });
        }
    }

    /// <summary>
    /// Logout and revoke refresh token + blacklist access token
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            await _authRepository.RevokeRefreshTokenAsync(request.RefreshToken);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                await _authRepository.LogAuditEventAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "LOGOUT",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Success = true
                });
            }

            Response.Cookies.Delete("refreshToken");

            _logger.LogInformation("User logged out successfully");

            // Blacklist the current access token so it cannot be reused until expiry
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                var blacklistKey = $"blacklist_{token}";
                await _cacheService.SetAsync(blacklistKey, "revoked", TimeSpan.FromHours(24));
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Get current user info (protected endpoint example)
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            // Define Cache Key
            var cacheKey = $"user_profile_{userId}";

            // Try to get from Cache (Fast Path)
            var user = await _cacheService.GetAsync<User>(cacheKey);
            if (user == null)
            {
                // Cache Miss - Get from DB (Slow Path)
                _logger.LogInformation("Cache miss for user profile: {UserId}. Fetching from DB.", userId);
                user = await _authRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Save to Cache for 30 minutes
                await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
            }

            return Ok(new
            {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private async Task CompleteEmailVerificationAsync(User user)
    {
        // Invalidate user profile cache now that their verification state changed
        await _cacheService.RemoveAsync($"user_profile_{user.Id}");
    }

    private async Task<IActionResult> BuildEmailVerificationResponseAsync(User user)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        
        await _authRepository.CreateRefreshTokenAsync(refreshToken);
        SetRefreshTokenCookie(refreshTokenValue);

        await _authRepository.LogAuditEventAsync(new AuditLog
        {
            UserId = user.Id,
            Action = "EMAIL_VERIFIED",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"].ToString(),
            Success = true
        });

        return Ok(new
        {
            message = "Email verified successfully! You are now logged in.",
            accessToken,
            refreshToken = refreshTokenValue,
            user = new { user.Id, user.Email, user.FirstName, user.LastName, user.Role }
        });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Force secure in production
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
