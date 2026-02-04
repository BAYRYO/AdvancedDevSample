using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Repositories;

public class EfCategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public EfCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Categories.FindAsync(id);
        return entity == null ? null : CategoryMapper.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        var entities = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        return entities.Select(CategoryMapper.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Category>> GetActiveAsync()
    {
        var entities = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return entities.Select(CategoryMapper.ToDomain).ToList();
    }

    public async Task SaveAsync(Category category)
    {
        var entity = CategoryMapper.ToEntity(category);
        var existing = await _context.Categories.FindAsync(category.Id);

        if (existing == null)
        {
            _context.Categories.Add(entity);
        }
        else
        {
            existing.Name = entity.Name;
            existing.Description = entity.Description;
            existing.IsActive = entity.IsActive;
            existing.UpdatedAt = entity.UpdatedAt;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Categories.FindAsync(id);
        if (entity != null)
        {
            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }
}
