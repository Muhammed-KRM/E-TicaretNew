using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<OrderResultDto>> CreateOrder(OrderCreateDto dto)
    {
        var (userId, guestId) = GetCustomerIdentity();
        var result = await _orderService.CreateOrderAsync(userId, guestId, dto);
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orders = await _orderService.GetMyOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var (userId, guestId) = GetCustomerIdentity();
        var order = await _orderService.GetOrderAsync(id, userId, guestId);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    [AllowAnonymous]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var (userId, guestId) = GetCustomerIdentity();
        await _orderService.CancelOrderAsync(id, userId, guestId);
        return Ok();
    }

    [HttpPost("{id:guid}/return")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestReturn(Guid id, [FromBody] ReturnRequestDto dto)
    {
        var (userId, guestId) = GetCustomerIdentity();
        await _orderService.RequestReturnAsync(id, userId, guestId, dto.ReturnReason);
        return Ok(new { message = "İade talebiniz alınmıştır." });
    }

    private (Guid? userId, string? guestId) GetCustomerIdentity()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId))
            return (userId, null);

        var guestId = Request.Headers["X-Guest-Id"].FirstOrDefault();
        return (null, guestId);
    }
}
