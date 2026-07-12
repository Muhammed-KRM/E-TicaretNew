using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var (userId, guestId) = GetCustomerIdentity();
        var cart = await _cartService.GetCartAsync(userId, guestId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] CartItemAddDto dto)
    {
        var (userId, guestId) = GetCustomerIdentity();
        await _cartService.AddItemAsync(userId, guestId, dto.ProductId, dto.Quantity);
        return Ok();
    }

    [HttpPut("items/{productId:guid}")]
    public async Task<IActionResult> UpdateQuantity(Guid productId, [FromBody] int quantity)
    {
        var (userId, guestId) = GetCustomerIdentity();
        await _cartService.UpdateQuantityAsync(userId, guestId, productId, quantity);
        return Ok();
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId)
    {
        var (userId, guestId) = GetCustomerIdentity();
        await _cartService.RemoveItemAsync(userId, guestId, productId);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var (userId, guestId) = GetCustomerIdentity();
        await _cartService.ClearCartAsync(userId, guestId);
        return Ok();
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetItemCount()
    {
        var (userId, guestId) = GetCustomerIdentity();
        var count = await _cartService.GetCartItemCountAsync(userId, guestId);
        return Ok(count);
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

public record CartItemAddDto(Guid ProductId, int Quantity);
