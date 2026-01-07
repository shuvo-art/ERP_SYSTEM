namespace Auth.Core.DTOs;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
