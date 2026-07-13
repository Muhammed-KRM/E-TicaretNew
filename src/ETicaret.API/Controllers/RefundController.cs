using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.API.Controllers;

/// <summary>
/// İade (Refund) yönetimi için API endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RefundController : ControllerBase
{
    private readonly IRefundService _refundService;
    private readonly ILogger<RefundController> _logger;

    public RefundController(IRefundService refundService, ILogger<RefundController> logger)
    {
        _refundService = refundService;
        _logger = logger;
    }

    #region User Endpoints

    /// <summary>
    /// İade talebi oluşturur (Kullanıcı)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRefund([FromBody] CreateRefundRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var result = await _refundService.CreateRefundAsync(userId, request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının tüm iade taleplerini getirir
    /// </summary>
    [HttpGet("my-refunds")]
    public async Task<ActionResult<List<RefundDto>>> GetMyRefunds()
    {
        var userId = GetUserId();
        var refunds = await _refundService.GetUserRefundsAsync(userId);
        return Ok(refunds);
    }

    /// <summary>
    /// Kullanıcının belirli bir iade talebini getirir
    /// </summary>
    [HttpGet("{refundId:guid}")]
    public async Task<ActionResult<RefundDto>> GetRefund(Guid refundId)
    {
        var userId = GetUserId();
        var refund = await _refundService.GetRefundByIdAsync(refundId);

        if (refund == null)
            return NotFound(new { message = "İade talebi bulunamadı" });

        // Sadece kendi iade talebini görebilir (admin değilse)
        if (refund.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        return Ok(refund);
    }

    /// <summary>
    /// Siparişin iade edilebilir olup olmadığını kontrol eder
    /// </summary>
    [HttpGet("check-eligibility/{orderId:guid}")]
    public async Task<ActionResult<bool>> CheckRefundEligibility(Guid orderId)
    {
        var userId = GetUserId();
        var canCreate = await _refundService.CanCreateRefundAsync(orderId, userId);
        return Ok(new { canCreate });
    }

    /// <summary>
    /// Sipariş için iade talebinin olup olmadığını kontrol eder
    /// </summary>
    [HttpGet("check-exists/{orderId:guid}")]
    public async Task<ActionResult<RefundDto>> GetRefundByOrder(Guid orderId)
    {
        var userId = GetUserId();
        var refund = await _refundService.GetRefundByOrderIdAsync(orderId);
        
        if (refund == null)
            return NotFound(new { message = "Bu sipariş için iade talebi bulunamadı" });
            
        // Sadece kendi iade talebini görebilir (admin değilse)
        if (refund.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        return Ok(refund);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Tüm iade taleplerini getirir (Admin - Pagination)
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<RefundDto>>> GetAllRefunds([FromQuery] RefundFilterDto filter)
    {
        var refunds = await _refundService.GetRefundsAsync(filter);
        return Ok(refunds);
    }

    /// <summary>
    /// Bekleyen iade taleplerini getirir (Admin)
    /// </summary>
    [HttpGet("admin/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<RefundDto>>> GetPendingRefunds([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var filter = new RefundFilterDto
        {
            Status = "Pending",
            Page = page,
            PageSize = pageSize
        };
        var refunds = await _refundService.GetRefundsAsync(filter);
        return Ok(refunds);
    }

    /// <summary>
    /// İade talebini onaylar (Admin)
    /// </summary>
    [HttpPost("{refundId:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveRefund(Guid refundId, [FromBody] ApproveRefundRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = GetUserId();
        var result = await _refundService.ApproveRefundAsync(refundId, adminId, request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// İade talebini reddeder (Admin)
    /// </summary>
    [HttpPost("{refundId:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectRefund(Guid refundId, [FromBody] RejectRefundRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "Red nedeni gereklidir" });

        var adminId = GetUserId();
        var result = await _refundService.RejectRefundAsync(refundId, adminId, request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// İade işlemini tamamlar (Admin - Para iadesi yapıldı)
    /// </summary>
    [HttpPost("{refundId:guid}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CompleteRefund(Guid refundId, [FromBody] CompleteRefundRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = GetUserId();
        var result = await _refundService.CompleteRefundAsync(refundId, adminId, request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    #endregion

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");

        return userId;
    }
}
