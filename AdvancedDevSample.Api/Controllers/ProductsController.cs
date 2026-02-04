using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDevSample.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound(new { title = "Ressource introuvable", detail = $"Le produit avec l'identifiant '{id}' est introuvable." });
        }
        return Ok(product);
    }

    /// <summary>
    /// Searches products with filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] ProductSearchRequest request)
    {
        var result = await _productService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Updates a product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productService.UpdateAsync(id, request);
        return Ok(product);
    }

    /// <summary>
    /// Changes the price of a product. (Backward compatible endpoint)
    /// </summary>
    [HttpPut("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ChangePrice(Guid id, [FromBody] ChangePriceRequest request)
    {
        _productService.ChangePrice(id, request);
        return NoContent();
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Applies a discount to a product.
    /// </summary>
    [HttpPost("{id:guid}/discount")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyDiscount(Guid id, [FromBody] ApplyDiscountRequest request)
    {
        var product = await _productService.ApplyDiscountAsync(id, request);
        return Ok(product);
    }

    /// <summary>
    /// Removes the discount from a product.
    /// </summary>
    [HttpDelete("{id:guid}/discount")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDiscount(Guid id)
    {
        var product = await _productService.RemoveDiscountAsync(id);
        return Ok(product);
    }

    /// <summary>
    /// Gets the price history for a product.
    /// </summary>
    [HttpGet("{id:guid}/price-history")]
    [ProducesResponseType(typeof(IReadOnlyList<PriceHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPriceHistory(Guid id)
    {
        var history = await _productService.GetPriceHistoryAsync(id);
        return Ok(history);
    }

    /// <summary>
    /// Activates a product.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var product = await _productService.ActivateAsync(id);
        return Ok(product);
    }

    /// <summary>
    /// Deactivates a product.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var product = await _productService.DeactivateAsync(id);
        return Ok(product);
    }
}
