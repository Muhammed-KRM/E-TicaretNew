using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto?> GetBySlugAsync(string slug);
    Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId);
    Task<CategoryDto> CreateAsync(CategoryCreateDto dto);
    Task<CategoryDto> UpdateAsync(int id, CategoryUpdateDto dto);
    Task DeleteAsync(int id);
}
