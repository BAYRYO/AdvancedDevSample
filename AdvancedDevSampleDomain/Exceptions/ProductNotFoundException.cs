namespace AdvancedDevSample.Domain.Exceptions;

public class ProductNotFoundException : DomainException
{
    public Guid ProductId { get; }

    public ProductNotFoundException(Guid productId)
        : base($"Le produit avec l'identifiant '{productId}' est introuvable.")
    {
        ProductId = productId;
    }
}
