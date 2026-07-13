using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaret.Business.DTOs;

/// <summary>
/// İade talebi görüntüleme DTO'su
/// </summary>
public class RefundDto
{
    /// <summary>
    /// İade talebi ID'si
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Sipariş ID'si
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Sipariş numarası (görüntüleme için)
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Kullanıcı adı
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// İade nedeni (Defective, WrongItem, NotAsDescribed, ChangedMind, Other)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Kullanıcının iade açıklaması
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// İade durumu (Pending, Approved, Rejected, Refunded)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Admin notları
    /// </summary>
    public string? AdminNotes { get; set; }
    
    /// <summary>
    /// İade edilecek tutar
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? RefundAmount { get; set; }
    
    /// <summary>
    /// İade talebinin oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// İade talebinin işlenme tarihi
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// İşleyen admin adı
    /// </summary>
    public string? ProcessedByName { get; set; }
}

/// <summary>
/// İade talebi oluşturma request DTO'su
/// </summary>
public class CreateRefundRequest
{
    /// <summary>
    /// İade edilecek sipariş ID'si
    /// </summary>
    [Required(ErrorMessage = "Sipariş ID'si zorunludur")]
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// İade nedeni
    /// Defective (Kusurlu ürün)
    /// WrongItem (Yanlış ürün)
    /// NotAsDescribed (Açıklamaya uygun değil)
    /// ChangedMind (Vazgeçtim)
    /// Other (Diğer)
    /// </summary>
    [Required(ErrorMessage = "İade nedeni zorunludur")]
    [StringLength(50)]
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// İade açıklaması (detaylı)
    /// </summary>
    [Required(ErrorMessage = "İade açıklaması zorunludur")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "İade açıklaması en az 10, en fazla 1000 karakter olmalıdır")]
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// İade talebini onaylama request DTO'su (Admin)
/// </summary>
public class ApproveRefundRequest
{
    /// <summary>
    /// Admin notları (isteğe bağlı)
    /// </summary>
    [StringLength(1000, ErrorMessage = "Admin notları en fazla 1000 karakter olabilir")]
    public string? AdminNotes { get; set; }
    
    /// <summary>
    /// İade edilecek tutar
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "İade tutarı 0'dan büyük olmalıdır")]
    public decimal RefundAmount { get; set; }
}

/// <summary>
/// İade talebini reddetme request DTO'su (Admin)
/// </summary>
public class RejectRefundRequest
{
    /// <summary>
    /// Reddetme nedeni
    /// </summary>
    [Required(ErrorMessage = "Reddetme nedeni zorunludur")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Reddetme nedeni en az 10, en fazla 1000 karakter olmalıdır")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// İade işlemi tamamlama request DTO'su (Admin)
/// Para iadesi yapıldığında kullanılır
/// </summary>
public class CompleteRefundRequest
{
    /// <summary>
    /// İşlem notları
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// İade işlemi sonucu DTO'su
/// </summary>
public class RefundResult
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
    /// Oluşturulan iade talebi ID'si (başarılı ise)
    /// </summary>
    public Guid? RefundId { get; set; }
}

/// <summary>
/// İade talebi listesi için filtre parametreleri
/// </summary>
public class RefundFilterDto
{
    /// <summary>
    /// Kullanıcı ID'si (admin için filtreleme)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Durum filtresi (Pending, Approved, Rejected, Refunded)
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Başlangıç tarihi
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Bitiş tarihi
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Sayfa numarası
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Sayfa başına kayıt sayısı
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
