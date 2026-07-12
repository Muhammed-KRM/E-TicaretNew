using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId);
    Task AddItemAsync(Guid userId, Guid productId, int quantity);
    Task UpdateQuantityAsync(Guid userId, Guid productId, int quantity);
    Task RemoveItemAsync(Guid userId, Guid productId);
    Task ClearCartAsync(Guid userId);
    Task<int> GetCartItemCountAsync(Guid userId);
}
