using ETicaret.Business.DTOs;
using ETicaret.Data.Enums;

namespace ETicaret.Business.Interfaces;

public interface IOrderService
{
    Task<OrderResultDto> CreateOrderAsync(Guid userId, OrderCreateDto dto);
    Task<List<OrderDto>> GetMyOrdersAsync(Guid userId);
    Task<OrderDto?> GetOrderAsync(Guid orderId, Guid userId);
    Task CancelOrderAsync(Guid orderId, Guid userId);
}
