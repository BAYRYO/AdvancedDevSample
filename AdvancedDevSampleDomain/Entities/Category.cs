using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.Entities;

/// <summary>
/// Entité représentant une catégorie de produits.
/// </summary>
public class Category
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Category(string name, string? description = null)
        : this(Guid.NewGuid(), name, description)
    {
    }

    public Category(Guid id, string name, string? description = null, bool isActive = true,
        DateTime? createdAt = null, DateTime? updatedAt = null)
    {
        ValidateName(name);

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = isActive;
        CreatedAt = createdAt ?? DateTime.UtcNow;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        if (description != null && description.Length > MaxDescriptionLength)
        {
            throw new DomainException($"La description ne peut pas depasser {MaxDescriptionLength} caracteres.");
        }

        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Le nom de la categorie est obligatoire.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new DomainException("Le nom de la categorie ne peut pas depasser 100 caracteres.");
        }
    }
}
