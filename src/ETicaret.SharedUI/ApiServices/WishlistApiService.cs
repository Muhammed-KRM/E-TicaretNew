using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.SharedUI.ApiServices;

public class WishlistApiService : IWishlistService
{
    private readonly HttpClient _http;
    
    public WishlistApiService(HttpClient http) => _http = http;

    public async Task<List<WishlistItemDto>> GetUserWishlistAsync(Guid userId)
    {
        try
        {
            var response = await _http.GetAsync("api/wishlist");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<WishlistItemDto>>() ?? new();
        }
        catch
        {
            return new List<WishlistItemDto>();
        }
    }

    public async Task<WishlistOperationResult> AddToWishlistAsync(Guid userId, Guid productId)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/wishlist/{productId}", new { });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WishlistOperationResult>() 
                ?? new WishlistOperationResult { Success = false };
        }
        catch
        {
            return new WishlistOperationResult { Success = false, Message = "Bir hata oluştu" };
        }
    }

    public async Task<WishlistOperationResult> RemoveFromWishlistAsync(Guid userId, Guid productId)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/wishlist/{productId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WishlistOperationResult>() 
                ?? new WishlistOperationResult { Success = false };
        }
        catch
        {
            return new WishlistOperationResult { Success = false, Message = "Bir hata oluştu" };
        }
    }

    public async Task<bool> IsInWishlistAsync(Guid userId, Guid productId)
    {
        try
        {
            var response = await _http.GetAsync($"api/wishlist/check/{productId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetWishlistCountAsync(Guid userId)
    {
        try
        {
            var response = await _http.GetAsync("api/wishlist/count");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }
        catch
        {
            return 0;
        }
    }

    public async Task<WishlistOperationResult> ClearWishlistAsync(Guid userId)
    {
        try
        {
            var response = await _http.DeleteAsync("api/wishlist/clear");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WishlistOperationResult>() 
                ?? new WishlistOperationResult { Success = false };
        }
        catch
        {
            return new WishlistOperationResult { Success = false, Message = "Bir hata oluştu" };
        }
    }

    public async Task<WishlistOperationResult> AddMultipleToWishlistAsync(Guid userId, List<Guid> productIds)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/wishlist/bulk", productIds);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WishlistOperationResult>() 
                ?? new WishlistOperationResult { Success = false };
        }
        catch
        {
            return new WishlistOperationResult { Success = false, Message = "Bir hata oluştu" };
        }
    }
}
