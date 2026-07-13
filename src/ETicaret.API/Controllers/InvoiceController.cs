using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.API.Controllers;

/// <summary>
/// Fatura (Invoice) yönetimi için API endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoiceController> _logger;

    public InvoiceController(
        IInvoiceService invoiceService,
        ILogger<InvoiceController> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    #region User Endpoints

    /// <summary>
    /// Sipariş için fatura oluşturur (Kullanıcı)
    /// </summary>
    [HttpPost("generate/{orderId:guid}")]
    public async Task<IActionResult> GenerateInvoice(Guid orderId)
    {
        var result = await _invoiceService.GenerateInvoiceAsync(orderId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Fatura PDF dosyasını indirir
    /// </summary>
    [HttpGet("download/{orderId:guid}")]
    public async Task<IActionResult> DownloadInvoice(Guid orderId)
    {
        var invoiceNumber = await _invoiceService.GetInvoiceNumberByOrderIdAsync(orderId);
        if (string.IsNullOrEmpty(invoiceNumber))
            return NotFound(new { message = "Bu sipariş için fatura bulunamadı" });

        var fileBytes = await _invoiceService.GenerateInvoicePdfAsync(orderId);

        if (fileBytes == null || fileBytes.Length == 0)
            return NotFound(new { message = "Fatura dosyası oluşturulamadı" });

        return File(fileBytes, "application/pdf", $"Fatura_{invoiceNumber}.pdf");
    }

    /// <summary>
    /// Kullanıcının ödeme geçmişini getirir
    /// </summary>
    [HttpGet("payment-history")]
    public async Task<ActionResult<PagedResult<PaymentHistoryDto>>> GetPaymentHistory(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var history = await _invoiceService.GetPaymentHistoryAsync(userId, page, pageSize);
        return Ok(history);
    }

    /// <summary>
    /// Sipariş için fatura bilgisini getirir
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoiceByOrder(Guid orderId)
    {
        var invoice = await _invoiceService.PrepareInvoiceDataAsync(orderId);
        return Ok(invoice);
    }

    /// <summary>
    /// Fatura numarasına göre fatura getirir
    /// </summary>
    [HttpGet("number/{invoiceNumber}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoiceByNumber(string invoiceNumber)
    {
        var invoice = await _invoiceService.GetInvoiceByNumberAsync(invoiceNumber);

        if (invoice == null)
            return NotFound(new { message = "Fatura bulunamadı" });

        return Ok(invoice);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Firma bilgilerini getirir (Admin)
    /// </summary>
    [HttpGet("admin/company-info")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CompanyInfoDto>> GetCompanyInfo()
    {
        var companyInfo = await _invoiceService.GetCompanyInfoAsync();
        return Ok(companyInfo);
    }

    /// <summary>
    /// Firma bilgilerini günceller (Admin)
    /// </summary>
    [HttpPut("admin/company-info")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCompanyInfo([FromBody] CompanyInfoDto companyInfo)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _invoiceService.UpdateCompanyInfoAsync(companyInfo);
        
        if (!result)
            return BadRequest(new { message = "Firma bilgileri güncellenemedi" });
            
        return Ok(new { message = "Firma bilgileri güncellendi" });
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
