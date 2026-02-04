namespace AdvancedDevSample.Domain.Exceptions;

public class DuplicateSkuException : DomainException
{
    public string Sku { get; }

    public DuplicateSkuException(string sku)
        : base($"Un produit avec le SKU '{sku}' existe deja.")
    {
        Sku = sku;
    }
}
