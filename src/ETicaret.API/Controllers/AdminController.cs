using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Entities;

namespace ETicaret.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers([FromQuery] string? search, [FromQuery] string? role, [FromQuery] string? status)
    {
        var users = await _adminService.GetAllUsersAsync(search, role, status);
        return Ok(users);
    }

    [HttpPost("users/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid id)
    {
        await _adminService.SuspendUserAsync(id);
        return Ok();
    }

    [HttpPost("users/{id:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        await _adminService.ActivateUserAsync(id);
        return Ok();
    }

    [HttpGet("products")]
    public async Task<ActionResult<List<AdminProductDto>>> GetProducts([FromQuery] string? search, [FromQuery] string? status)
    {
        var products = await _adminService.GetAllProductsAsync(search, status);
        return Ok(products);
    }

    [HttpPost("products/{id:guid}/approve")]
    public async Task<IActionResult> ApproveProduct(Guid id)
    {
        await _adminService.ApproveProductAsync(id);
        return Ok();
    }

    [HttpPost("products/{id:guid}/reject")]
    public async Task<IActionResult> RejectProduct(Guid id)
    {
        await _adminService.RejectProductAsync(id);
        return Ok();
    }

    [HttpPost("products/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendProduct(Guid id)
    {
        await _adminService.SuspendProductAsync(id);
        return Ok();
    }

    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _adminService.DeleteProductAsync(id);
        return Ok();
    }

    [HttpGet("orders")]
    public async Task<ActionResult<List<AdminOrderDto>>> GetOrders([FromQuery] string? search, [FromQuery] string? status)
    {
        var orders = await _adminService.GetAllOrdersAsync(search, status);
        return Ok(orders);
    }

    [HttpPut("orders/{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] string newStatus)
    {
        await _adminService.UpdateOrderStatusAsync(id, newStatus);
        return Ok();
    }

    // --- İADE YÖNETİMİ ---

    [HttpGet("returns")]
    public async Task<ActionResult<List<OrderDto>>> GetReturnRequests([FromServices] IOrderService orderService)
    {
        var orders = await orderService.GetReturnRequestsAsync();
        return Ok(orders);
    }

    [HttpPost("returns/{id:guid}/approve")]
    public async Task<IActionResult> ApproveReturn(Guid id, [FromBody] ProcessReturnDto dto, [FromServices] IOrderService orderService)
    {
        await orderService.ApproveReturnAsync(id, dto.AdminNote);
        return Ok(new { message = "İade onaylandı ve ödeme iadesi başlatıldı." });
    }

    [HttpPost("returns/{id:guid}/reject")]
    public async Task<IActionResult> RejectReturn(Guid id, [FromBody] ProcessReturnDto dto, [FromServices] IOrderService orderService)
    {
        await orderService.RejectReturnAsync(id, dto.AdminNote ?? "");
        return Ok(new { message = "İade talebi reddedildi." });
    }

    // --- KARGO YÖNETİMİ ---

    [HttpPost("orders/{id:guid}/shipping")]
    public async Task<IActionResult> UpdateShipping(Guid id, [FromBody] UpdateShippingDto dto, [FromServices] IOrderService orderService)
    {
        await orderService.UpdateShippingInfoAsync(id, dto);
        return Ok(new { message = "Kargo bilgisi güncellendi." });
    }

    // --- LOG YÖNETİMİ ---

    [HttpGet("logs/endpoints")]
    public async Task<ActionResult<List<EndpointLog>>> GetEndpointLogs(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? method, [FromQuery] string? path,
        [FromQuery] int? minStatusCode, [FromQuery] int? maxStatusCode,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromServices] ILogService logService = null!)
    {
        var logs = await logService.GetEndpointLogsAsync(
            from, to, method, path, minStatusCode, maxStatusCode, page, pageSize);
        return Ok(logs);
    }

    [HttpGet("logs/functions")]
    public async Task<ActionResult<List<FunctionLog>>> GetFunctionLogs(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? severity, [FromQuery] string? className,
        [FromQuery] bool? isSuccess,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromServices] ILogService logService = null!)
    {
        var logs = await logService.GetFunctionLogsAsync(
            from, to, severity, className, isSuccess, page, pageSize);
        return Ok(logs);
    }

    // --- İLETİŞİM YÖNETİMİ ---

    [HttpPut("contact/info")]
    public async Task<IActionResult> UpdateContactInfo(ContactInfoDto dto, [FromServices] IContactService contactService)
    {
        await contactService.UpdateContactInfoAsync(dto);
        return Ok();
    }

    [HttpGet("contact/messages")]
    public async Task<ActionResult<List<ContactMessageDto>>> GetContactMessages(
        [FromQuery] bool? isRead, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromServices] IContactService contactService = null!)
    {
        var messages = await contactService.GetMessagesAsync(isRead, page, pageSize);
        return Ok(messages);
    }

    [HttpPost("contact/messages/{id:int}/read")]
    public async Task<IActionResult> MarkMessageAsRead(int id, [FromServices] IContactService contactService)
    {
        await contactService.MarkAsReadAsync(id);
        return Ok();
    }

    [HttpPost("contact/messages/{id:int}/reply")]
    public async Task<IActionResult> ReplyToMessage(int id, [FromBody] ContactReplyDto dto, [FromServices] IContactService contactService)
    {
        await contactService.ReplyToMessageAsync(id, dto.Reply);
        return Ok();
    }
}
