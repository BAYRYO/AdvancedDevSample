using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.ValueObjects;

namespace AdvancedDevSample.Domain.Entities
{
    /// <summary>
    /// Représente un produit vendable.
    /// </summary>
    public class Product
    {
        public const int MaxNameLength = 200;
        public const int MaxDescriptionLength = 2000;

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public decimal Price { get; private set; }
        public bool IsActive { get; private set; }
        public Sku? Sku { get; private set; }
        public Stock Stock { get; private set; }
        public Guid? CategoryId { get; private set; }
        public Discount? CurrentDiscount { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Returns the price value for backward compatibility.
        /// </summary>
        public decimal PriceValue => Price;

        // Backward compatible constructors
        public Product(decimal price) : this(Guid.NewGuid(), price) { }

        public Product(Guid id, decimal price)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            Price = price;
            IsActive = true;
            Name = string.Empty;
            Stock = new Stock(0);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Product(Guid id, decimal price, bool isActive)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            Price = price;
            IsActive = isActive;
            Name = string.Empty;
            Stock = new Stock(0);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Full constructor for new products
        public Product(
            string name,
            decimal price,
            Sku sku,
            int stock = 0,
            string? description = null,
            Guid? categoryId = null)
        {
            ValidateName(name);

            Id = Guid.NewGuid();
            Name = name.Trim();
            Description = description?.Trim();
            Price = price;
            Sku = sku;
            Stock = new Stock(stock);
            CategoryId = categoryId;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Full constructor for reconstitution from persistence
        public Product(
            Guid id,
            string name,
            decimal price,
            Sku? sku,
            int stock,
            string? description,
            Guid? categoryId,
            decimal? discountPercentage,
            bool isActive,
            DateTime createdAt,
            DateTime updatedAt)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            Name = name ?? string.Empty;
            Description = description;
            Price = price;
            Sku = sku;
            Stock = new Stock(stock);
            CategoryId = categoryId;
            CurrentDiscount = discountPercentage.HasValue ? new Discount(discountPercentage.Value) : null;
            IsActive = isActive;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        /// <summary>
        /// Updates the price of the product to the specified value.
        /// </summary>
        public void ChangePrice(decimal newPrice)
        {
            if (newPrice <= 0)
            {
                throw new DomainException("Prix invalide");
            }

            if (!IsActive)
            {
                throw new DomainException("Produit inactif");
            }

            Price = newPrice;
            UpdatedAt = DateTime.UtcNow;
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

        public void UpdateCategory(Guid? categoryId)
        {
            CategoryId = categoryId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ApplyDiscount(decimal percentage, string? reason = null)
        {
            if (!IsActive)
            {
                throw new DomainException("Impossible d'appliquer une reduction a un produit inactif.");
            }

            CurrentDiscount = new Discount(percentage);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveDiscount()
        {
            CurrentDiscount = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public decimal GetEffectivePrice()
        {
            if (CurrentDiscount.HasValue)
            {
                return CurrentDiscount.Value.ApplyTo(Price);
            }

            return Price;
        }

        public void AddStock(int quantity)
        {
            Stock = Stock.Add(quantity);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveStock(int quantity)
        {
            Stock = Stock.Remove(quantity);
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

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Le nom du produit est obligatoire.");
            }

            if (name.Trim().Length > MaxNameLength)
            {
                throw new DomainException("Le nom du produit ne peut pas depasser 200 caracteres.");
            }
        }
    }
}
