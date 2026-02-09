using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.Entities;

public class User
{
    public sealed class ReconstitutionData
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string PasswordHash { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public UserRole Role { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public DateTime? LastLoginAt { get; init; }
    }

    public const int MaxEmailLength = 256;
    public const int MaxFirstNameLength = 100;
    public const int MaxLastNameLength = 100;

    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    // Constructor for creating new users
    public User(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        UserRole role = UserRole.User)
    {
        ValidateEmail(email);
        ValidateName(firstName, nameof(firstName), MaxFirstNameLength);
        ValidateName(lastName, nameof(lastName), MaxLastNameLength);

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Le hash du mot de passe est obligatoire.");
        }

        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Constructor for reconstitution from persistence
    public User(ReconstitutionData data)
    {
        Id = data.Id == Guid.Empty ? Guid.NewGuid() : data.Id;
        Email = data.Email;
        PasswordHash = data.PasswordHash;
        FirstName = data.FirstName;
        LastName = data.LastName;
        Role = data.Role;
        IsActive = data.IsActive;
        CreatedAt = data.CreatedAt;
        UpdatedAt = data.UpdatedAt;
        LastLoginAt = data.LastLoginAt;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string firstName, string lastName)
    {
        ValidateName(firstName, nameof(firstName), MaxFirstNameLength);
        ValidateName(lastName, nameof(lastName), MaxLastNameLength);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new DomainException("Le hash du mot de passe est obligatoire.");
        }

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("L'adresse email est obligatoire.");
        }

        if (email.Length >= MaxEmailLength)
        {
            throw new DomainException($"L'adresse email ne peut pas depasser {MaxEmailLength} caracteres.");
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            throw new DomainException("L'adresse email n'est pas valide.");
        }

        var domain = email.Substring(atIndex + 1);
        if (!domain.Contains('.'))
        {
            throw new DomainException("L'adresse email n'est pas valide.");
        }
    }

    private static void ValidateName(string name, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException($"Le champ {fieldName} est obligatoire.");
        }

        if (name.Trim().Length > maxLength)
        {
            throw new DomainException($"Le champ {fieldName} ne peut pas depasser {maxLength} caracteres.");
        }
    }
}
