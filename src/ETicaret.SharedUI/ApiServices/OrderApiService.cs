using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.SharedUI.ApiServices;

public class OrderApiService : IOrderService
{
    private readonly HttpClient _http;

    public OrderApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<OrderResultDto> CreateOrderAsync(Guid? userId, string? guestId, OrderCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/orders", dto);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<OrderResultDto>();
            return result ?? new OrderResultDto(false, Guid.Empty, "", "");
        }
        return new OrderResultDto(false, Guid.Empty, "", "");
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync(Guid userId)
    {
        return await _http.GetFromJsonAsync<List<OrderDto>>($"api/orders/my-orders") ?? new();
    }

    public async Task<OrderDto?> GetOrderAsync(Guid orderId, Guid? userId = null, string? guestId = null)
    {
        return await _http.GetFromJsonAsync<OrderDto>($"api/orders/{orderId}");
    }

    public async Task CancelOrderAsync(Guid orderId, Guid? userId = null, string? guestId = null)
    {
        var response = await _http.PostAsync($"api/orders/{orderId}/cancel", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task RequestReturnAsync(Guid orderId, Guid? userId, string? guestId, string returnReason)
    {
        var dto = new ReturnRequestDto { ReturnReason = returnReason };
        var response = await _http.PostAsJsonAsync($"api/orders/{orderId}/return", dto);
        response.EnsureSuccessStatusCode();
    }

    // --- Admin Metotları ---

    public async Task<List<OrderDto>> GetReturnRequestsAsync()
    {
        return await _http.GetFromJsonAsync<List<OrderDto>>("api/admin/orders/returns") ?? new();
    }

    public async Task ApproveReturnAsync(Guid orderId, string? adminNote = null)
    {
        var dto = new ProcessReturnDto { AdminNote = adminNote };
        var response = await _http.PostAsJsonAsync($"api/admin/orders/{orderId}/return/approve", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task RejectReturnAsync(Guid orderId, string adminNote)
    {
        var dto = new ProcessReturnDto { AdminNote = adminNote };
        var response = await _http.PostAsJsonAsync($"api/admin/orders/{orderId}/return/reject", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateShippingInfoAsync(Guid orderId, UpdateShippingDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/orders/{orderId}/shipping", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateOrderStatusAdminAsync(Guid orderId, UpdateOrderStatusDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/orders/{orderId}/status", dto.NewStatus.ToString());
        response.EnsureSuccessStatusCode();
    }
}
