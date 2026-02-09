using AdvancedDevSample.Application.Validators;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Test.Application;

public class PasswordValidatorTests
{
    [Fact]
    public void Validate_WithStrongPassword_DoesNotThrow()
    {
        var exception = Record.Exception(() => PasswordValidator.Validate("StrongPass123!"));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithMissingPassword_ThrowsWeakPasswordException()
    {
        var exception = Assert.Throws<WeakPasswordException>(() => PasswordValidator.Validate(" "));

        Assert.Contains("Password is required.", exception.ValidationErrors);
    }

    [Fact]
    public void Validate_WithWeakPassword_ThrowsAllRelevantErrors()
    {
        var exception = Assert.Throws<WeakPasswordException>(() => PasswordValidator.Validate("abc"));

        Assert.Contains($"Password must be at least {PasswordValidator.MinLength} characters long.", exception.ValidationErrors);
        Assert.Contains("Password must contain at least one uppercase letter.", exception.ValidationErrors);
        Assert.Contains("Password must contain at least one digit.", exception.ValidationErrors);
        Assert.Contains("Password must contain at least one special character (!@#$%^&*(),.?\":{}|<>).", exception.ValidationErrors);
    }

    [Fact]
    public void IsValid_WithStrongPassword_ReturnsTrueAndNoErrors()
    {
        var isValid = PasswordValidator.IsValid("ValidPass123!", out var errors);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_WithEmptyPassword_ReturnsFalseWithRequiredError()
    {
        var isValid = PasswordValidator.IsValid("", out var errors);

        Assert.False(isValid);
        Assert.Single(errors);
        Assert.Equal("Password is required.", errors[0]);
    }

    [Fact]
    public void IsValid_WithTooLongPassword_ReturnsFalseWithMaxLengthError()
    {
        var longPassword = new string('A', PasswordValidator.MaxLength + 1) + "1!a";

        var isValid = PasswordValidator.IsValid(longPassword, out var errors);

        Assert.False(isValid);
        Assert.Contains($"Password must be at most {PasswordValidator.MaxLength} characters long.", errors);
    }
}
