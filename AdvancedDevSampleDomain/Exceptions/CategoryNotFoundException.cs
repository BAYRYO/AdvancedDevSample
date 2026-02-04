namespace AdvancedDevSample.Domain.Exceptions;

public class CategoryNotFoundException : DomainException
{
    public Guid CategoryId { get; }

    public CategoryNotFoundException(Guid categoryId)
        : base($"La categorie avec l'identifiant '{categoryId}' est introuvable.")
    {
        CategoryId = categoryId;
    }
}
