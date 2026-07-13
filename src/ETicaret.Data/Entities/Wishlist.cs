using System.ComponentModel.DataAnnotations;

namespace ETicaret.Data.Entities;

/// <summary>
/// Kullanıcının favori ürün listesi
/// Bir kullanıcı bir ürünü sadece bir kez favoriye ekleyebilir
/// </summary>
public class Wishlist
{
    /// <summary>
    /// Benzersiz kimlik
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Kullanıcı kimliği
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Ürün kimliği
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Favoriye eklenme tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili kullanıcı
    /// </summary>
    public User User { get; set; } = null!;
    
    /// <summary>
    /// İlişkili ürün
    /// </summary>
    public Product Product { get; set; } = null!;
}
