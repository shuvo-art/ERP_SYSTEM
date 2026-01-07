using Auth.Core.Entities;
using System.Security.Claims;

namespace Auth.Core.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    int? GetUserIdFromToken(string token);
}
