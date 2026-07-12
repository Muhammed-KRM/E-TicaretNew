using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.SharedUI.ApiServices;

public class CategoryApiService : ICategoryService
{
    private readonly HttpClient _http;

    public CategoryApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<CategoryDto>>("api/categories") ?? new List<CategoryDto>();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<CategoryDto>($"api/categories/{id}");
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        return await _http.GetFromJsonAsync<CategoryDto>($"api/categories/slug/{slug}");
    }

    public async Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId)
    {
        return await _http.GetFromJsonAsync<List<CategoryDto>>($"api/categories/{parentId}/subcategories") ?? new List<CategoryDto>();
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/categories", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? throw new Exception("Kategori oluşturulamadı.");
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/categories/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? throw new Exception("Kategori güncellenemedi.");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/categories/{id}");
        response.EnsureSuccessStatusCode();
    }
}
