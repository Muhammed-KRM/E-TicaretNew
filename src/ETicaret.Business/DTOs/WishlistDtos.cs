using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaret.Business.DTOs;

/// <summary>
/// Wishlist item görüntüleme DTO'su
/// Favorilerdeki ürünlerin detaylı bilgilerini içerir
/// </summary>
public class WishlistItemDto
{
    /// <summary>
    /// Wishlist kaydının ID'si
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Ürün ID'si
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Ürün başlığı
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Ürün görseli URL'i
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Ürün fiyatı
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    /// <summary>
    /// İndirimli fiyat (varsa)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountedPrice { get; set; }
    
    /// <summary>
    /// Ürün stokta mı?
    /// </summary>
    public bool IsInStock { get; set; }
    
    /// <summary>
    /// Favorilere eklenme tarihi
    /// </summary>
    public DateTime AddedAt { get; set; }
    
    /// <summary>
    /// Ortalama puan
    /// </summary>
    public double AverageRating { get; set; }
    
    /// <summary>
    /// Yorum sayısı
    /// </summary>
    public int ReviewCount { get; set; }
}

/// <summary>
/// Ürünü favorilere ekleme request DTO'su
/// </summary>
public class AddToWishlistRequest
{
    /// <summary>
    /// Favoriye eklenecek ürün ID'si
    /// </summary>
    [Required(ErrorMessage = "Ürün ID'si zorunludur")]
    public Guid ProductId { get; set; }
}

/// <summary>
/// Wishlist sayısı response DTO'su
/// Navbar badge için kullanılır
/// </summary>
public class WishlistCountDto
{
    /// <summary>
    /// Kullanıcının favori listesindeki ürün sayısı
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// Wishlist işlem sonucu DTO'su
/// </summary>
public class WishlistOperationResult
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
    /// Güncel favori sayısı
    /// </summary>
    public int CurrentCount { get; set; }
}
