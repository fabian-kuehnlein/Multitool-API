using Multitool.Application.Models;

namespace Multitool.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
}
