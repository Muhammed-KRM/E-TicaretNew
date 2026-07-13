using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.API.Controllers;

/// <summary>
/// Wishlist (Favori Listesi) yönetimi için API endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
    {
        _wishlistService = wishlistService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcının wishlist'ini getirir
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<WishlistItemDto>>> GetWishlist()
    {
        var userId = GetUserId();
        var wishlist = await _wishlistService.GetUserWishlistAsync(userId);
        return Ok(wishlist);
    }

    /// <summary>
    /// Wishlist'e ürün ekler
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var result = await _wishlistService.AddToWishlistAsync(userId, request.ProductId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Wishlist'ten ürün çıkarır
    /// </summary>
    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId)
    {
        var userId = GetUserId();
        var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId);

        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = result.Message, currentCount = result.CurrentCount });
    }

    /// <summary>
    /// Ürünün wishlist'te olup olmadığını kontrol eder
    /// </summary>
    [HttpGet("check/{productId:guid}")]
    public async Task<ActionResult<bool>> IsInWishlist(Guid productId)
    {
        var userId = GetUserId();
        var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, productId);
        return Ok(new { isInWishlist });
    }

    /// <summary>
    /// Wishlist'teki toplam ürün sayısını getirir
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<WishlistCountDto>> GetWishlistCount()
    {
        var userId = GetUserId();
        var count = await _wishlistService.GetWishlistCountAsync(userId);
        return Ok(count);
    }

    /// <summary>
    /// Wishlist'i temizler
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearWishlist()
    {
        var userId = GetUserId();
        await _wishlistService.ClearWishlistAsync(userId);
        return Ok(new { message = "Favori listesi temizlendi" });
    }

    /// <summary>
    /// Toplu ürün ekleme (birden fazla ürünü wishlist'e ekler)
    /// </summary>
    [HttpPost("bulk")]
    public async Task<IActionResult> AddMultipleToWishlist([FromBody] List<Guid> productIds)
    {
        if (productIds == null || !productIds.Any())
            return BadRequest(new { message = "Ürün listesi boş olamaz" });

        var userId = GetUserId();
        var result = await _wishlistService.AddMultipleToWishlistAsync(userId, productIds);

        return Ok(new
        {
            message = result.Message,
            success = result.Success,
            currentCount = result.CurrentCount
        });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");

        return userId;
    }
}
