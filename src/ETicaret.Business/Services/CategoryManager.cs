using Microsoft.Extensions.Caching.Memory;
using ETicaret.Business.DTOs;
using ETicaret.Business.Exceptions;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Entities;
using ETicaret.Data.Repositories;

namespace ETicaret.Business.Services;

public class CategoryManager : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IMemoryCache _cache;
    private const string CacheKeyAll = "categories_all";

    public CategoryManager(ICategoryRepository categoryRepo, IMemoryCache cache)
    {
        _categoryRepo = categoryRepo;
        _cache = cache;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        if (_cache.TryGetValue(CacheKeyAll, out List<CategoryDto>? cachedCategories) && cachedCategories != null)
        {
            return cachedCategories;
        }

        var categories = await _categoryRepo.GetAllAsync();
        var result = categories.Select(MapToDto).ToList();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1)); // 1 saat cache
        _cache.Set(CacheKeyAll, result, cacheOptions);

        return result;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var categories = await _categoryRepo.FindAsync(c => c.Id == id);
        var category = categories.FirstOrDefault();
        return category is null ? null : MapToDto(category);
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        var categories = await _categoryRepo.FindAsync(c => c.Slug == slug);
        var category = categories.FirstOrDefault();
        return category is null ? null : MapToDto(category);
    }

    public async Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId)
    {
        var categories = await _categoryRepo.FindAsync(c => c.ParentCategoryId == parentId);
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Slug = ETicaret.Business.Helpers.SlugHelper.GenerateSlug(dto.Name),
            Description = dto.Description,
            IconUrl = dto.IconUrl,
            ParentCategoryId = dto.ParentCategoryId,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        var categories = await _categoryRepo.FindAsync(c => c.Id == id);
        var category = categories.FirstOrDefault() ?? throw new NotFoundException("Kategori", id);

        category.Name = dto.Name;
        category.Slug = ETicaret.Business.Helpers.SlugHelper.GenerateSlug(dto.Name);
        category.Description = dto.Description;
        category.IconUrl = dto.IconUrl;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;

        _categoryRepo.Update(category);
        await _categoryRepo.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task DeleteAsync(int id)
    {
        var categories = await _categoryRepo.FindAsync(c => c.Id == id);
        var category = categories.FirstOrDefault() ?? throw new NotFoundException("Kategori", id);

        _categoryRepo.Delete(category);
        await _categoryRepo.SaveChangesAsync();
    }

    private static CategoryDto MapToDto(Category c) => new(
        c.Id, c.Name, c.Slug, c.Description, c.IconUrl, c.ParentCategoryId, c.DisplayOrder, c.IsActive
    );
}
