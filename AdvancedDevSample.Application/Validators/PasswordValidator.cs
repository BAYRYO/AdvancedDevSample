using System.Text.RegularExpressions;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Application.Validators;

/// <summary>
/// Validates password strength according to security best practices.
/// </summary>
public static class PasswordValidator
{
    public const int MinLength = 8;
    public const int MaxLength = 128;
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly string[] RequiredPasswordErrors = ["Password is required."];

    /// <summary>
    /// Validates a password against security requirements.
    /// Throws WeakPasswordException if validation fails.
    /// </summary>
    public static void Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new WeakPasswordException(RequiredPasswordErrors);
        }

        if (password.Length < MinLength)
        {
            errors.Add($"Password must be at least {MinLength} characters long.");
        }

        if (password.Length > MaxLength)
        {
            errors.Add($"Password must be at most {MaxLength} characters long.");
        }

        if (!Regex.IsMatch(password, @"[A-Z]", RegexOptions.None, RegexTimeout))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (!Regex.IsMatch(password, @"[a-z]", RegexOptions.None, RegexTimeout))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        if (!Regex.IsMatch(password, @"[0-9]", RegexOptions.None, RegexTimeout))
        {
            errors.Add("Password must contain at least one digit.");
        }

        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]", RegexOptions.None, RegexTimeout))
        {
            errors.Add("Password must contain at least one special character (!@#$%^&*(),.?\":{}|<>).");
        }

        if (errors.Count > 0)
        {
            throw new WeakPasswordException(errors);
        }
    }

    /// <summary>
    /// Checks if a password meets all requirements without throwing.
    /// </summary>
    public static bool IsValid(string password, out IReadOnlyList<string> errors)
    {
        var errorList = new List<string>();
        errors = errorList;

        if (string.IsNullOrWhiteSpace(password))
        {
            errorList.Add("Password is required.");
            return false;
        }

        if (password.Length < MinLength)
        {
            errorList.Add($"Password must be at least {MinLength} characters long.");
        }

        if (password.Length > MaxLength)
        {
            errorList.Add($"Password must be at most {MaxLength} characters long.");
        }

        if (!Regex.IsMatch(password, @"[A-Z]", RegexOptions.None, RegexTimeout))
        {
            errorList.Add("Password must contain at least one uppercase letter.");
        }

        if (!Regex.IsMatch(password, @"[a-z]", RegexOptions.None, RegexTimeout))
        {
            errorList.Add("Password must contain at least one lowercase letter.");
        }

        if (!Regex.IsMatch(password, @"[0-9]", RegexOptions.None, RegexTimeout))
        {
            errorList.Add("Password must contain at least one digit.");
        }

        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]", RegexOptions.None, RegexTimeout))
        {
            errorList.Add("Password must contain at least one special character.");
        }

        return errorList.Count == 0;
    }
}
