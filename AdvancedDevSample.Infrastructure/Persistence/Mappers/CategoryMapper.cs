using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Infrastructure.Persistence.Entities;

namespace AdvancedDevSample.Infrastructure.Persistence.Mappers;

public static class CategoryMapper
{
    public static Category ToDomain(CategoryEntity entity)
    {
        return new Category(
            id: entity.Id,
            name: entity.Name,
            description: entity.Description,
            isActive: entity.IsActive,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt);
    }

    public static CategoryEntity ToEntity(Category category)
    {
        return new CategoryEntity
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
