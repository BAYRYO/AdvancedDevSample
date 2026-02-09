using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Test.Domain.Entities;

public class UserTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidData_CreatesUser()
    {
        // Act
        var user = new User(
            email: "test@example.com",
            passwordHash: "hashed_password",
            firstName: "John",
            lastName: "Doe");

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("hashed_password", user.PasswordHash);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal(UserRole.User, user.Role);
        Assert.True(user.IsActive);
        Assert.Equal("John Doe", user.FullName);
    }

    [Fact]
    public void Constructor_WithAdminRole_CreatesAdminUser()
    {
        // Act
        var user = new User(
            email: "admin@example.com",
            passwordHash: "hashed_password",
            firstName: "Admin",
            lastName: "User",
            role: UserRole.Admin);

        // Assert
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public void Constructor_NormalizesEmail()
    {
        // Act
        var user = new User(
            email: "  TEST@EXAMPLE.COM  ",
            passwordHash: "hashed_password",
            firstName: "John",
            lastName: "Doe");

        // Assert
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public void Constructor_TrimsNames()
    {
        // Act
        var user = new User(
            email: "test@example.com",
            passwordHash: "hashed_password",
            firstName: "  John  ",
            lastName: "  Doe  ");

        // Assert
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyEmail_ThrowsDomainException(string? email)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new User(
            email: email!,
            passwordHash: "hash",
            firstName: "John",
            lastName: "Doe"));
    }

    [Fact]
    public void Constructor_WithEmailTooLong_ThrowsDomainException()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@b.com";

        // Act & Assert
        Assert.Throws<DomainException>(() => new User(
            email: longEmail,
            passwordHash: "hash",
            firstName: "John",
            lastName: "Doe"));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("test.example.com")]
    public void Constructor_WithInvalidEmailFormat_ThrowsDomainException(string email)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new User(
            email: email,
            passwordHash: "hash",
            firstName: "John",
            lastName: "Doe"));
    }

    #endregion

    #region Name Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyFirstName_ThrowsDomainException(string? firstName)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: firstName!,
            lastName: "Doe"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyLastName_ThrowsDomainException(string? lastName)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: "John",
            lastName: lastName!));
    }

    [Fact]
    public void Constructor_WithFirstNameTooLong_ThrowsDomainException()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act & Assert
        Assert.Throws<DomainException>(() => new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: longName,
            lastName: "Doe"));
    }

    [Fact]
    public void Constructor_WithLastNameTooLong_ThrowsDomainException()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act & Assert
        Assert.Throws<DomainException>(() => new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: "John",
            lastName: longName));
    }

    #endregion

    #region Password Hash Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyPasswordHash_ThrowsDomainException(string? passwordHash)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new User(
            email: "test@example.com",
            passwordHash: passwordHash!,
            firstName: "John",
            lastName: "Doe"));
    }

    #endregion

    #region Method Tests

    [Fact]
    public void UpdateLastLogin_SetsLastLoginAt()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: "John",
            lastName: "Doe");
        var beforeUpdate = DateTime.UtcNow;

        // Act
        user.UpdateLastLogin();

        // Assert
        Assert.NotNull(user.LastLoginAt);
        Assert.True(user.LastLoginAt >= beforeUpdate);
    }

    [Fact]
    public void UpdateName_UpdatesNames()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: "John",
            lastName: "Doe");

        // Act
        user.UpdateName("Jane", "Smith");

        // Assert
        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Smith", user.LastName);
    }

    [Fact]
    public void ChangeRole_UpdatesRole()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: "John",
            lastName: "Doe");

        // Act
        user.ChangeRole(UserRole.Admin);

        // Assert
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: "John",
            lastName: "Doe");

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: "John",
            lastName: "Doe");
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void ChangePassword_UpdatesPasswordHash()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "old_hash",
            firstName: "John",
            lastName: "Doe");

        // Act
        user.ChangePassword("new_hash");

        // Assert
        Assert.Equal("new_hash", user.PasswordHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangePassword_WithEmptyHash_ThrowsDomainException(string? newHash)
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hash",
            firstName: "John",
            lastName: "Doe");

        // Act & Assert
        Assert.Throws<DomainException>(() => user.ChangePassword(newHash!));
    }

    #endregion

    [Fact]
    public void Reconstitution_Constructor_WithEmptyId_GeneratesNewId()
    {
        // Act
        var user = new User(new User.ReconstitutionData
        {
            Id = Guid.Empty,
            Email = "reconstituted@example.com",
            PasswordHash = "hash",
            FirstName = "Recon",
            LastName = "User",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            LastLoginAt = null
        });

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("reconstituted@example.com", user.Email);
    }
}
