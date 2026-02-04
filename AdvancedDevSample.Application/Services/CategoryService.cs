using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Application.Services;

public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category(request.Name, request.Description);
        await _categoryRepository.SaveAsync(category);

        return ToCategoryResponse(category);
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category == null ? null : ToCategoryResponse(category);
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(ToCategoryResponse).ToList();
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetActiveAsync()
    {
        var categories = await _categoryRepository.GetActiveAsync();
        return categories.Select(ToCategoryResponse).ToList();
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new CategoryNotFoundException(id);

        if (request.Name != null)
        {
            category.UpdateName(request.Name);
        }

        if (request.Description != null)
        {
            category.UpdateDescription(request.Description);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                category.Activate();
            }
            else
            {
                category.Deactivate();
            }
        }

        await _categoryRepository.SaveAsync(category);

        return ToCategoryResponse(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new CategoryNotFoundException(id);

        await _categoryRepository.DeleteAsync(id);
    }

    private static CategoryResponse ToCategoryResponse(Category category)
    {
        return new CategoryResponse(
            Id: category.Id,
            Name: category.Name,
            Description: category.Description,
            IsActive: category.IsActive,
            CreatedAt: category.CreatedAt,
            UpdatedAt: category.UpdatedAt);
    }
}
