using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth.Core.DTOs;
using Auth.Core.Entities;
using Auth.Core.Interfaces;
using FluentValidation;
using Shared.Kernel.Interfaces;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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

    public AuthController(
        IAuthRepository authRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IEmailService emailService,
        IOtpService otpService)
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
    /// Verify email with OTP
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpRequest request)
    {
        try
        {
            var user = await _authRepository.VerifyEmailOTPAsync(request.Email, request.Otp);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid or expired OTP" });
            }

            // Generate tokens
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

            // Audit log
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return StatusCode(500, new { message = "Internal server error during verification" });
        }
    }

    /// <summary>
    /// Login and get access token
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

            // Get user by email
            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
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

            // Check if account is locked
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
                await _authRepository.RecordFailedLoginAsync(request.Email);
                await _authRepository.LogAuditEventAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "LOGIN_FAILED",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Details = "Invalid password",
                    Success = false
                });
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check if email is verified
            if (!user.IsEmailVerified)
            {
                // Resend OTP
                var otp = _otpService.GenerateOtp();
                var expiry = DateTime.UtcNow.AddMinutes(15);
                await _authRepository.SetEmailVerificationOTPAsync(user.Id, otp, expiry);

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Verify Your Email",
                    $"Your verification code is: <strong>{otp}</strong>. It expires in 15 minutes."
                );

                return StatusCode(403, new { 
                    message = "Please verify your email address. A new OTP has been sent.",
                    email = user.Email
                });
            }

            // Reset failed login attempts on successful login
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
    /// Request password reset OTP
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

            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                // Return Ok to prevent user enumeration
                return Ok(new { message = "If the email exists, a password reset OTP has been sent" });
            }

            var otp = _otpService.GenerateOtp();
            var expiry = DateTime.UtcNow.AddMinutes(15);
            
            await _authRepository.SetPasswordResetOTPAsync(user.Id, otp, expiry);

            // Audit log
            await _authRepository.LogAuditEventAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "PASSWORD_RESET_REQUESTED",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Success = true
            });

            _logger.LogInformation("Password reset OTP for {Email}: {Otp}", user.Email, otp);
            
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
    /// Reset password using OTP
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

            var isValid = await _authRepository.VerifyPasswordResetOTPAsync(request.Email, request.Otp);
            if (!isValid)
            {
                return BadRequest(new { message = "Invalid or expired OTP" });
            }

            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null) return NotFound(new { message = "User not found" });

            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            await _authRepository.UpdatePasswordAsync(user.Id, newPasswordHash);

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
    /// Logout and revoke refresh token
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

            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role,
                createdAt = user.CreatedAt,
                lastLoginAt = user.LastLoginAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred" });
        }
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
