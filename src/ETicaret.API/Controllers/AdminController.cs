using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

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
}
