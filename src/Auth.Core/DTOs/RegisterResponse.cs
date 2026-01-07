namespace Auth.Core.DTOs;

public class RegisterResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Message { get; set; } = "User registered successfully";
}
