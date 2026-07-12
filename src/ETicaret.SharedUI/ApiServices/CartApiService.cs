using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.SharedUI.ApiServices;

public class CartApiService : ICartService
{
    private readonly HttpClient _http;
    public CartApiService(HttpClient http) => _http = http;
    public async Task<CartDto> GetCartAsync(Guid? userId, string? guestId) => default!;
    public async Task AddItemAsync(Guid? userId, string? guestId, Guid productId, int quantity) => await Task.CompletedTask;
    public async Task UpdateQuantityAsync(Guid? userId, string? guestId, Guid productId, int quantity) => await Task.CompletedTask;
    public async Task RemoveItemAsync(Guid? userId, string? guestId, Guid productId) => await Task.CompletedTask;
    public async Task ClearCartAsync(Guid? userId, string? guestId) => await Task.CompletedTask;
    public async Task<int> GetCartItemCountAsync(Guid? userId, string? guestId) => 0;
}
