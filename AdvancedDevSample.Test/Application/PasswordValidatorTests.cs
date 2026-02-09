using AdvancedDevSample.Application.Validators;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Test.Application;

public class PasswordValidatorTests
{
    [Fact]
    public void Validate_WithStrongPassword_DoesNotThrow()
    {
        Exception? exception = Record.Exception(() => PasswordValidator.Validate("StrongPass123!"));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithMissingPassword_ThrowsWeakPasswordException()
    {
        WeakPasswordException exception = Assert.Throws<WeakPasswordException>(() => PasswordValidator.Validate(" "));

        Assert.Contains("Password is required.", exception.ValidationErrors);
    }

    [Fact]
    public void Validate_WithWeakPassword_ThrowsAllRelevantErrors()
    {
        WeakPasswordException exception = Assert.Throws<WeakPasswordException>(() => PasswordValidator.Validate("abc"));

        Assert.Contains($"Password must be at least {PasswordValidator.MinLength} characters long.", exception.ValidationErrors);
        Assert.Contains("Password must contain at least one uppercase letter.", exception.ValidationErrors);
        Assert.Contains("Password must contain at least one digit.", exception.ValidationErrors);
        Assert.Contains("Password must contain at least one special character (!@#$%^&*(),.?\":{}|<>).", exception.ValidationErrors);
    }

    [Fact]
    public void IsValid_WithStrongPassword_ReturnsTrueAndNoErrors()
    {
        bool isValid = PasswordValidator.IsValid("ValidPass123!", out IReadOnlyList<string> errors);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_WithEmptyPassword_ReturnsFalseWithRequiredError()
    {
        bool isValid = PasswordValidator.IsValid("", out IReadOnlyList<string> errors);

        Assert.False(isValid);
        Assert.Single(errors);
        Assert.Equal("Password is required.", errors[0]);
    }

    [Fact]
    public void IsValid_WithTooLongPassword_ReturnsFalseWithMaxLengthError()
    {
        string longPassword = new string('A', PasswordValidator.MaxLength + 1) + "1!a";

        bool isValid = PasswordValidator.IsValid(longPassword, out IReadOnlyList<string> errors);

        Assert.False(isValid);
        Assert.Contains($"Password must be at most {PasswordValidator.MaxLength} characters long.", errors);
    }

    [Fact]
    public void IsValid_WithOnlyLowercaseLetters_ReturnsAllMissingComplexityErrors()
    {
        bool isValid = PasswordValidator.IsValid("onlylowercase", out IReadOnlyList<string> errors);

        Assert.False(isValid);
        Assert.Contains("Password must contain at least one uppercase letter.", errors);
        Assert.Contains("Password must contain at least one digit.", errors);
        Assert.Contains("Password must contain at least one special character.", errors);
    }
}
