using Auth.Core.Interfaces;
using System.Text.RegularExpressions;

namespace Auth.Infrastructure.Services;

public class PasswordValidator : IPasswordValidator
{
    private readonly int _minimumLength = 8;
    private readonly Regex _uppercaseRegex = new Regex(@"[A-Z]");
    private readonly Regex _lowercaseRegex = new Regex(@"[a-z]");
    private readonly Regex _digitRegex = new Regex(@"[0-9]");
    private readonly Regex _specialCharRegex = new Regex(@"[@$!%*?&#]");

    // Common passwords to reject
    private readonly HashSet<string> _commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "password", "password123", "12345678", "qwerty", "abc123", 
        "monkey", "1234567890", "letmein", "trustno1", "dragon",
        "baseball", "iloveyou", "master", "sunshine", "ashley",
        "bailey", "passw0rd", "shadow", "123123", "654321"
    };

    public (bool IsValid, List<string> Errors) ValidatePassword(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required");
            return (false, errors);
        }

        if (password.Length < _minimumLength)
        {
            errors.Add($"Password must be at least {_minimumLength} characters long");
        }

        if (!_uppercaseRegex.IsMatch(password))
        {
            errors.Add("Password must contain at least one uppercase letter");
        }

        if (!_lowercaseRegex.IsMatch(password))
        {
            errors.Add("Password must contain at least one lowercase letter");
        }

        if (!_digitRegex.IsMatch(password))
        {
            errors.Add("Password must contain at least one digit");
        }

        if (!_specialCharRegex.IsMatch(password))
        {
            errors.Add("Password must contain at least one special character (@$!%*?&#)");
        }

        if (_commonPasswords.Contains(password))
        {
            errors.Add("This password is too common. Please choose a more secure password");
        }

        return (errors.Count == 0, errors);
    }
}
