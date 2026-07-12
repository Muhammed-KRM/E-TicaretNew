using ETicaret.Business.DTOs;
using ETicaret.Data.Enums;

namespace ETicaret.Business.Interfaces;

public interface IOrderService
{
    Task<OrderResultDto> CreateOrderAsync(Guid? userId, string? guestId, OrderCreateDto dto);
    Task<List<OrderDto>> GetMyOrdersAsync(Guid userId);
    Task<OrderDto?> GetOrderAsync(Guid orderId, Guid? userId, string? guestId);
    Task CancelOrderAsync(Guid orderId, Guid? userId, string? guestId);
    
    // İade Talebi (Kullanıcı)
    Task RequestReturnAsync(Guid orderId, Guid? userId, string? guestId, string returnReason);

    // Admin İade Yönetimi
    Task<List<OrderDto>> GetReturnRequestsAsync();
    Task ApproveReturnAsync(Guid orderId, string? adminNote);
    Task RejectReturnAsync(Guid orderId, string adminNote);
    
    // Kargo Yönetimi
    Task UpdateShippingInfoAsync(Guid orderId, UpdateShippingDto dto);
    Task UpdateOrderStatusAdminAsync(Guid orderId, UpdateOrderStatusDto dto);
}
