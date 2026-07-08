using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();
    }
}
