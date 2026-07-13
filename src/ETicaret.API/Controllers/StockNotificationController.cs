using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.API.Controllers;

/// <summary>
/// Stok bildirimi yönetimi için API endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StockNotificationController : ControllerBase
{
    private readonly IStockNotificationService _stockNotificationService;
    private readonly ILogger<StockNotificationController> _logger;

    public StockNotificationController(
        IStockNotificationService stockNotificationService, 
        ILogger<StockNotificationController> logger)
    {
        _stockNotificationService = stockNotificationService;
        _logger = logger;
    }

    #region Public Endpoints

    /// <summary>
    /// Stok bildirimine abone olur (Giriş yapmış kullanıcı veya misafir)
    /// </summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeStockNotificationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _stockNotificationService.SubscribeAsync(request.ProductId, request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının aktif stok bildirimlerini getirir (Giriş yapmış kullanıcı)
    /// </summary>
    [HttpGet("my-subscriptions")]
    [Authorize]
    public async Task<ActionResult<List<StockNotificationDto>>> GetMySubscriptions([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email adresi gereklidir" });
            
        var subscriptions = await _stockNotificationService.GetNotificationsByEmailAsync(email);
        return Ok(subscriptions);
    }

    /// <summary>
    /// Stok bildirimi aboneliğini iptal eder
    /// </summary>
    [HttpDelete("{notificationId:guid}")]
    public async Task<IActionResult> Unsubscribe(Guid notificationId)
    {
        // Bu metod için silme işlemi yapan bir servis metodu yok, manuel silme eklenebilir
        return NotFound(new { message = "Bu özellik henüz desteklenmiyor" });
    }

    /// <summary>
    /// Kullanıcının belirli bir ürün için aktif bildirimi olup olmadığını kontrol eder
    /// </summary>
    [HttpGet("check/{productId:guid}")]
    public async Task<ActionResult<bool>> HasPendingNotification(Guid productId, [FromQuery] string? email = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email gereklidir" });

        var hasPending = await _stockNotificationService.HasPendingNotificationAsync(productId, email);
        return Ok(new { hasPending });
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Bekleyen tüm stok bildirimlerini getirir (Admin - Pagination)
    /// </summary>
    [HttpGet("admin/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<StockNotificationDto>>> GetPendingNotifications(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50)
    {
        var notifications = await _stockNotificationService.GetPendingNotificationsAsync(page, pageSize);
        return Ok(notifications);
    }

    /// <summary>
    /// Belirli bir ürün için bekleyen bildirimleri getirir (Admin)
    /// </summary>
    [HttpGet("admin/product/{productId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<StockNotificationDto>>> GetProductNotifications(Guid productId)
    {
        var notifications = await _stockNotificationService.GetNotificationsByProductAsync(productId);
        return Ok(notifications);
    }

    /// <summary>
    /// Toplu stok bildirimi email'i gönderir (Admin)
    /// </summary>
    [HttpPost("admin/send-bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendBulkNotifications([FromBody] SendStockNotificationsRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _stockNotificationService.SendNotificationsForProductAsync(request.ProductId);

        return Ok(new
        {
            message = $"{result.SentCount} bildirim gönderildi",
            sentCount = result.SentCount,
            failedCount = result.FailedCount
        });
    }

    /// <summary>
    /// Tek bir stok bildirimini manuel olarak gönderir (Admin)
    /// </summary>
    [HttpPost("admin/send/{notificationId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendSingleNotification(Guid notificationId)
    {
        var result = await _stockNotificationService.SendSingleNotificationAsync(notificationId);

        if (!result.Success)
            return NotFound(new { message = "Stok bildirimi bulunamadı veya gönderilemedi" });

        return Ok(new { message = "Stok bildirimi gönderildi" });
    }

    /// <summary>
    /// Eski (90+ gün) stok bildirimlerini temizler (Admin - Maintenance)
    /// </summary>
    [HttpDelete("admin/cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupOldNotifications()
    {
        var deletedCount = await _stockNotificationService.CleanupOldNotificationsAsync();
        return Ok(new { message = $"{deletedCount} eski bildirim temizlendi", deletedCount });
    }

    #endregion

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");

        return userId;
    }

    private Guid? GetUserIdOrNull()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }
}
