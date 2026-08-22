using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await db.Categories.FindAsync(id);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<int> CreateCategoryAsync(Category category)
    {
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return category.Id;
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        db.Categories.Update(category);
        await db.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Category category)
    {
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
    }
}
