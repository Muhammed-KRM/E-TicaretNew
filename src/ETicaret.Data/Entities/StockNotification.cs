using System.ComponentModel.DataAnnotations;

namespace ETicaret.Data.Entities;

/// <summary>
/// Stok bildirim talepleri entity'si
/// Ürün stoğa girdiğinde kullanıcıya email bildirimi gönderilir
/// </summary>
public class StockNotification
{
    /// <summary>
    /// Benzersiz kimlik
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Stok bildirimi istenilen ürün kimliği
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Bildirim gönderilecek email adresi
    /// </summary>
    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Bildirim gönderildi mi?
    /// </summary>
    public bool IsNotified { get; set; } = false;
    
    /// <summary>
    /// Bildirimin gönderilme tarihi
    /// </summary>
    public DateTime? NotifiedAt { get; set; }
    
    /// <summary>
    /// Kaydın oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Property
    
    /// <summary>
    /// Stok bildirimi istenilen ürün
    /// </summary>
    public Product Product { get; set; } = null!;
}
