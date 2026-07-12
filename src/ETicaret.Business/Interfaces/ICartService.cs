using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid? userId, string? guestId);
    Task AddItemAsync(Guid? userId, string? guestId, Guid productId, int quantity);
    Task UpdateQuantityAsync(Guid? userId, string? guestId, Guid productId, int quantity);
    Task RemoveItemAsync(Guid? userId, string? guestId, Guid productId);
    Task ClearCartAsync(Guid? userId, string? guestId);
    Task<int> GetCartItemCountAsync(Guid? userId, string? guestId);
}
