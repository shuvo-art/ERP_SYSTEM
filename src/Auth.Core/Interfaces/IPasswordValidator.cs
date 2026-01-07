namespace Auth.Core.Interfaces;

public interface IPasswordValidator
{
    (bool IsValid, List<string> Errors) ValidatePassword(string password);
}
