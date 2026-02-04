using System.Net;
using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Domain.ValueObjects;

namespace AdvancedDevSample.Application.Services;

public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IPriceHistoryRepository priceHistoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _priceHistoryRepository = priceHistoryRepository;
    }

    // Keep existing sync method for backward compatibility
    public void ChangePrice(Guid id, ChangePriceRequest request)
    {
        var product = _productRepository.GetById(id)
            ?? throw new ApplicationServiceException("Produit introuvable", HttpStatusCode.NotFound);

        product.ChangePrice(request.NewPrice);
        _productRepository.Save(product);
    }

    // New async methods
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var sku = new Sku(request.Sku);

        if (await _productRepository.ExistsWithSkuAsync(sku.Value))
        {
            throw new DuplicateSkuException(sku.Value);
        }

        if (request.CategoryId.HasValue && !await _categoryRepository.ExistsAsync(request.CategoryId.Value))
        {
            throw new CategoryNotFoundException(request.CategoryId.Value);
        }

        var product = new Product(
            name: request.Name,
            price: request.Price,
            sku: sku,
            stock: request.Stock,
            description: request.Description,
            categoryId: request.CategoryId);

        await _productRepository.SaveAsync(product);

        return await GetProductResponseAsync(product);
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return null;
        }

        return await GetProductResponseAsync(product);
    }

    public async Task<PagedResponse<ProductResponse>> SearchAsync(ProductSearchRequest request)
    {
        var criteria = new ProductSearchCriteria(
            Name: request.Name,
            MinPrice: request.MinPrice,
            MaxPrice: request.MaxPrice,
            CategoryId: request.CategoryId,
            IsActive: request.IsActive,
            Page: request.Page,
            PageSize: request.PageSize);

        var (items, totalCount) = await _productRepository.SearchAsync(criteria);

        var responses = new List<ProductResponse>();
        foreach (var product in items)
        {
            responses.Add(await GetProductResponseAsync(product));
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResponse<ProductResponse>(
            Items: responses,
            TotalCount: totalCount,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalPages: totalPages);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new ProductNotFoundException(id);

        if (request.Name != null)
        {
            product.UpdateName(request.Name);
        }

        if (request.Description != null)
        {
            product.UpdateDescription(request.Description);
        }

        if (request.Price.HasValue)
        {
            var oldPrice = product.Price;
            product.ChangePrice(request.Price.Value);

            var priceHistory = new PriceHistory(
                productId: product.Id,
                oldPrice: oldPrice,
                newPrice: request.Price.Value,
                discountPercentage: null,
                reason: "Mise a jour du prix");
            await _priceHistoryRepository.SaveAsync(priceHistory);
        }

        if (request.Stock.HasValue)
        {
            var currentStock = product.Stock.Quantity;
            var diff = request.Stock.Value - currentStock;
            if (diff > 0)
            {
                product.AddStock(diff);
            }
            else if (diff < 0)
            {
                product.RemoveStock(-diff);
            }
        }

        if (request.CategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(request.CategoryId.Value))
            {
                throw new CategoryNotFoundException(request.CategoryId.Value);
            }
            product.UpdateCategory(request.CategoryId);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                product.Activate();
            }
            else
            {
                product.Deactivate();
            }
        }

        await _productRepository.SaveAsync(product);

        return await GetProductResponseAsync(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new ProductNotFoundException(id);

        await _productRepository.DeleteAsync(id);
    }

    public async Task<ProductResponse> ApplyDiscountAsync(Guid id, ApplyDiscountRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new ProductNotFoundException(id);

        var oldEffectivePrice = product.GetEffectivePrice();

        product.ApplyDiscount(request.Percentage, request.Reason);

        var newEffectivePrice = product.GetEffectivePrice();

        var priceHistory = new PriceHistory(
            productId: product.Id,
            oldPrice: oldEffectivePrice,
            newPrice: newEffectivePrice,
            discountPercentage: request.Percentage,
            reason: request.Reason ?? "Application d'une reduction");
        await _priceHistoryRepository.SaveAsync(priceHistory);

        await _productRepository.SaveAsync(product);

        return await GetProductResponseAsync(product);
    }

    public async Task<ProductResponse> RemoveDiscountAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new ProductNotFoundException(id);

        if (product.CurrentDiscount.HasValue)
        {
            var oldEffectivePrice = product.GetEffectivePrice();
            var discountPct = product.CurrentDiscount.Value.Percentage;

            product.RemoveDiscount();

            var newEffectivePrice = product.GetEffectivePrice();

            var priceHistory = new PriceHistory(
                productId: product.Id,
                oldPrice: oldEffectivePrice,
                newPrice: newEffectivePrice,
                discountPercentage: 0,
                reason: "Suppression de la reduction");
            await _priceHistoryRepository.SaveAsync(priceHistory);

            await _productRepository.SaveAsync(product);
        }

        return await GetProductResponseAsync(product);
    }

    public async Task<IReadOnlyList<PriceHistoryResponse>> GetPriceHistoryAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new ProductNotFoundException(id);

        var history = await _priceHistoryRepository.GetByProductIdAsync(id);

        return history.Select(h => new PriceHistoryResponse(
            Id: h.Id,
            ProductId: h.ProductId,
            OldPrice: h.OldPrice,
            NewPrice: h.NewPrice,
            DiscountPercentage: h.DiscountPercentage,
            ChangedAt: h.ChangedAt,
            Reason: h.Reason)).ToList();
    }

    public async Task<ProductResponse> ActivateAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new ProductNotFoundException(id);

        product.Activate();
        await _productRepository.SaveAsync(product);

        return await GetProductResponseAsync(product);
    }

    public async Task<ProductResponse> DeactivateAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new ProductNotFoundException(id);

        product.Deactivate();
        await _productRepository.SaveAsync(product);

        return await GetProductResponseAsync(product);
    }

    private async Task<ProductResponse> GetProductResponseAsync(Product product)
    {
        string? categoryName = null;
        if (product.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId.Value);
            categoryName = category?.Name;
        }

        return new ProductResponse(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description,
            Sku: product.Sku?.Value,
            Price: product.Price,
            EffectivePrice: product.GetEffectivePrice(),
            DiscountPercentage: product.CurrentDiscount?.Percentage,
            Stock: product.Stock.Quantity,
            CategoryId: product.CategoryId,
            CategoryName: categoryName,
            IsActive: product.IsActive,
            CreatedAt: product.CreatedAt,
            UpdatedAt: product.UpdatedAt);
    }
}
