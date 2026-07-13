using System.ComponentModel.DataAnnotations;

namespace ETicaret.Business.DTOs;

/// <summary>
/// Stok bildirimi görüntüleme DTO'su
/// Admin panelinde stok bildirim taleplerini listelemek için kullanılır
/// </summary>
public class StockNotificationDto
{
    /// <summary>
    /// Bildirim kaydı ID'si
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Ürün ID'si
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Ürün adı
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Ürün görseli
    /// </summary>
    public string ProductImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Bildirim gönderilecek email
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Bildirim gönderildi mi?
    /// </summary>
    public bool IsNotified { get; set; }
    
    /// <summary>
    /// Bildirimin gönderilme tarihi
    /// </summary>
    public DateTime? NotifiedAt { get; set; }
    
    /// <summary>
    /// Kaydın oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Stok bildirimine abone olma request DTO'su
/// </summary>
public class SubscribeStockNotificationRequest
{
    /// <summary>
    /// Ürün ID'si
    /// </summary>
    [Required(ErrorMessage = "Ürün ID'si zorunludur")]
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Bildirim gönderilecek email adresi
    /// </summary>
    [Required(ErrorMessage = "Email adresi zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [StringLength(256, ErrorMessage = "Email adresi en fazla 256 karakter olabilir")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Stok bildirimi işlem sonucu DTO'su
/// </summary>
public class StockNotificationResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// İşlem sonuç mesajı
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Toplu stok bildirimi gönderme request DTO'su (Admin/Worker)
/// </summary>
public class SendStockNotificationsRequest
{
    /// <summary>
    /// Stok bildirimi gönderilecek ürün ID'si
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Bildirim gönderilecek maksimum kayıt sayısı (varsayılan: 1000)
    /// Rate limiting için kullanılır
    /// </summary>
    [Range(1, 10000)]
    public int MaxCount { get; set; } = 1000;
}

/// <summary>
/// Stok bildirimi gönderme sonucu DTO'su
/// </summary>
public class SendStockNotificationsResult
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
    /// Başarıyla gönderilen bildirim sayısı
    /// </summary>
    public int SentCount { get; set; }
    
    /// <summary>
    /// Başarısız bildirim sayısı
    /// </summary>
    public int FailedCount { get; set; }
}
