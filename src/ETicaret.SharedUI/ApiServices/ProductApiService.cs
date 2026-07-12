using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;
using System.Threading;

namespace ETicaret.SharedUI.ApiServices;

public class ProductApiService : IProductService
{
    private readonly HttpClient _http;
    public ProductApiService(HttpClient http) => _http = http;

    public async Task<ProductSearchResultDto> SearchAsync(ProductSearchFilterDto filters, CancellationToken ct = default)
    {
        try
        {
            var query = $"api/products/search?page={filters.Page}&pageSize={filters.PageSize}";
            if (!string.IsNullOrEmpty(filters.Query))    query += $"&query={Uri.EscapeDataString(filters.Query)}";
            if (filters.CategoryId.HasValue)              query += $"&categoryId={filters.CategoryId}";
            if (!string.IsNullOrEmpty(filters.Brand))    query += $"&brand={Uri.EscapeDataString(filters.Brand)}";
            if (filters.MinPrice.HasValue)                query += $"&minPrice={filters.MinPrice}";
            if (filters.MaxPrice.HasValue)                query += $"&maxPrice={filters.MaxPrice}";
            if (filters.InStock.HasValue)                 query += $"&inStock={filters.InStock}";
            if (filters.IsFeatured.HasValue)              query += $"&isFeatured={filters.IsFeatured}";
            if (!string.IsNullOrEmpty(filters.SortBy))   query += $"&sortBy={filters.SortBy}";

            return await _http.GetFromJsonAsync<ProductSearchResultDto>(query, ct) ?? new ProductSearchResultDto();
        }
        catch { return new ProductSearchResultDto(); }
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        try { return await _http.GetFromJsonAsync<ProductDto>($"api/products/{id}"); }
        catch { return null; }
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        try { return await _http.GetFromJsonAsync<ProductDto>($"api/products/slug/{slug}"); }
        catch { return null; }
    }

    public async Task<List<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        try { return await _http.GetFromJsonAsync<List<ProductDto>>($"api/products/category/{categoryId}") ?? new(); }
        catch { return new(); }
    }

    public async Task<List<ProductDto>> GetFeaturedAsync()
    {
        try { return await _http.GetFromJsonAsync<List<ProductDto>>("api/products/featured") ?? new(); }
        catch { return new(); }
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/products", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>() ?? throw new Exception("Ürün oluşturulamadı.");
    }

    public async Task<ProductDto> UpdateAsync(Guid id, ProductUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/products/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>() ?? throw new Exception("Ürün güncellenemedi.");
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/products/{id}");
        response.EnsureSuccessStatusCode();
    }
}
