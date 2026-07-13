using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaret.Business.DTOs;

/// <summary>
/// Fatura ana DTO'su
/// PDF oluşturma ve görüntüleme için kullanılır
/// </summary>
public class InvoiceDto
{
    /// <summary>
    /// Fatura numarası
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Sipariş numarası
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Fatura tarihi
    /// </summary>
    public DateTime InvoiceDate { get; set; }
    
    /// <summary>
    /// Firma bilgileri
    /// </summary>
    public CompanyInfoDto CompanyInfo { get; set; } = new();
    
    /// <summary>
    /// Müşteri bilgileri
    /// </summary>
    public CustomerInfoDto CustomerInfo { get; set; } = new();
    
    /// <summary>
    /// Fatura kalemleri (ürünler)
    /// </summary>
    public List<InvoiceItemDto> Items { get; set; } = new();
    
    /// <summary>
    /// Ara toplam (vergi hariç)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    
    /// <summary>
    /// KDV tutarı
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// Kargo ücreti
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingCost { get; set; }
    
    /// <summary>
    /// İndirim tutarı
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }
    
    /// <summary>
    /// Genel toplam (vergiler dahil)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Ödeme yöntemi
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Notlar (isteğe bağlı)
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Firma bilgileri DTO'su
/// </summary>
public class CompanyInfoDto
{
    /// <summary>
    /// Firma adı
    /// </summary>
    public string Name { get; set; } = "OzelDers E-Ticaret";
    
    /// <summary>
    /// Firma adresi
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Vergi dairesi
    /// </summary>
    public string TaxOffice { get; set; } = string.Empty;
    
    /// <summary>
    /// Vergi numarası
    /// </summary>
    public string TaxNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Website
    /// </summary>
    public string Website { get; set; } = "www.ozelders.com";
    
    /// <summary>
    /// Logo URL'i (isteğe bağlı)
    /// </summary>
    public string? LogoUrl { get; set; }
}

/// <summary>
/// Müşteri bilgileri DTO'su
/// </summary>
public class CustomerInfoDto
{
    /// <summary>
    /// Müşteri adı soyadı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// Adres satırı
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// İl
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// İlçe
    /// </summary>
    public string District { get; set; } = string.Empty;
    
    /// <summary>
    /// Posta kodu
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
    
    /// <summary>
    /// TC Kimlik No veya Vergi No (isteğe bağlı)
    /// </summary>
    public string? TaxIdOrIdentityNumber { get; set; }
}

/// <summary>
/// Fatura kalemi (ürün satırı) DTO'su
/// </summary>
public class InvoiceItemDto
{
    /// <summary>
    /// Ürün adı
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Ürün kodu (SKU)
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Adet
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Birim fiyat (vergi hariç)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Toplam fiyat (vergi hariç)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// KDV oranı (varsayılan: %20)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; } = 20.00m;
    
    /// <summary>
    /// KDV tutarı
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
}

/// <summary>
/// Ödeme geçmişi DTO'su
/// Kullanıcının tüm ödemelerini ve faturalarını listelemek için
/// </summary>
public class PaymentHistoryDto
{
    /// <summary>
    /// Ödeme transaction ID'si
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Sipariş ID'si
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Sipariş numarası
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Ödeme tarihi
    /// </summary>
    public DateTime PaymentDate { get; set; }
    
    /// <summary>
    /// Ödeme tutarı
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Ödeme yöntemi
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Ödeme durumu
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Fatura numarası (varsa)
    /// </summary>
    public string? InvoiceNumber { get; set; }
    
    /// <summary>
    /// Fatura indirilebilir mi?
    /// </summary>
    public bool CanDownloadInvoice { get; set; }
}

/// <summary>
/// Fatura oluşturma request DTO'su
/// </summary>
public class GenerateInvoiceRequest
{
    /// <summary>
    /// Sipariş ID'si
    /// </summary>
    [Required(ErrorMessage = "Sipariş ID'si zorunludur")]
    public Guid OrderId { get; set; }
}

/// <summary>
/// Fatura oluşturma sonucu DTO'su
/// </summary>
public class GenerateInvoiceResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// İşlem sonuç mesajı
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Fatura numarası
    /// </summary>
    public string? InvoiceNumber { get; set; }
    
    /// <summary>
    /// Fatura PDF dosya yolu veya URL'i
    /// </summary>
    public string? InvoiceUrl { get; set; }
}
