using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaret.Data.Entities;

/// <summary>
/// İade talepleri entity'si
/// 14 günlük yasal iade hakkı kapsamında oluşturulan talepler
/// </summary>
public class Refund
{
    /// <summary>
    /// Benzersiz kimlik
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// İlişkili sipariş kimliği
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// İade talebinde bulunan kullanıcı kimliği
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// İade nedeni (Defective, WrongItem, NotAsDescribed, ChangedMind, Other)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Kullanıcının iade talebi açıklaması
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// İade durumu (Pending, Approved, Rejected, Refunded)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = RefundStatus.Pending;
    
    /// <summary>
    /// Admin'in iade talebi hakkında notları
    /// </summary>
    [StringLength(1000)]
    public string? AdminNotes { get; set; }
    
    /// <summary>
    /// İade edilecek tutar
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? RefundAmount { get; set; }
    
    /// <summary>
    /// İade talebini işleyen admin kimliği
    /// </summary>
    public Guid? ProcessedBy { get; set; }
    
    /// <summary>
    /// İade talebinin işlenme tarihi
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// İade talebinin oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili sipariş
    /// </summary>
    public Order Order { get; set; } = null!;
    
    /// <summary>
    /// İade talebinde bulunan kullanıcı
    /// </summary>
    public User User { get; set; } = null!;
    
    /// <summary>
    /// İade talebini işleyen admin
    /// </summary>
    public User? ProcessedByUser { get; set; }
}

/// <summary>
/// İade nedenleri sabit değerleri
/// </summary>
public static class RefundReason
{
    /// <summary>
    /// Ürün kusurlu/hasarlı
    /// </summary>
    public const string Defective = "Defective";
    
    /// <summary>
    /// Yanlış ürün gönderildi
    /// </summary>
    public const string WrongItem = "WrongItem";
    
    /// <summary>
    /// Ürün açıklamaya uygun değil
    /// </summary>
    public const string NotAsDescribed = "NotAsDescribed";
    
    /// <summary>
    /// Kullanıcı fikrini değiştirdi
    /// </summary>
    public const string ChangedMind = "ChangedMind";
    
    /// <summary>
    /// Diğer nedenler
    /// </summary>
    public const string Other = "Other";
}

/// <summary>
/// İade durumları sabit değerleri
/// </summary>
public static class RefundStatus
{
    /// <summary>
    /// Onay bekliyor
    /// </summary>
    public const string Pending = "Pending";
    
    /// <summary>
    /// Admin tarafından onaylandı
    /// </summary>
    public const string Approved = "Approved";
    
    /// <summary>
    /// Admin tarafından reddedildi
    /// </summary>
    public const string Rejected = "Rejected";
    
    /// <summary>
    /// Para iadesi tamamlandı
    /// </summary>
    public const string Refunded = "Refunded";
}
