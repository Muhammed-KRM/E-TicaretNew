using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

/// <summary>
/// Wishlist (Favori Listesi) işlemleri için servis interface'i
/// </summary>
public interface IWishlistService
{
    /// <summary>
    /// Kullanıcının favori listesini getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>Favori ürün listesi</returns>
    Task<List<WishlistItemDto>> GetUserWishlistAsync(Guid userId);
    
    /// <summary>
    /// Ürünü favorilere ekler
    /// Zaten favorilerdeyse duplicate hata verir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="productId">Ürün ID'si</param>
    /// <returns>İşlem sonucu</returns>
    Task<WishlistOperationResult> AddToWishlistAsync(Guid userId, Guid productId);
    
    /// <summary>
    /// Ürünü favorilerden çıkarır
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="productId">Ürün ID'si</param>
    /// <returns>İşlem sonucu</returns>
    Task<WishlistOperationResult> RemoveFromWishlistAsync(Guid userId, Guid productId);
    
    /// <summary>
    /// Ürün favorilerde mi kontrol eder
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="productId">Ürün ID'si</param>
    /// <returns>True ise favorilerde</returns>
    Task<bool> IsInWishlistAsync(Guid userId, Guid productId);
    
    /// <summary>
    /// Favori sayısını döner (navbar badge için)
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>Favori sayısı</returns>
    Task<int> GetWishlistCountAsync(Guid userId);
    
    /// <summary>
    /// Tüm favori listesini temizler
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>İşlem sonucu</returns>
    Task<WishlistOperationResult> ClearWishlistAsync(Guid userId);
    
    /// <summary>
    /// Birden fazla ürünü favorilere ekler (sepetten favori oluşturma gibi senaryolar için)
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="productIds">Ürün ID listesi</param>
    /// <returns>İşlem sonucu</returns>
    Task<WishlistOperationResult> AddMultipleToWishlistAsync(Guid userId, List<Guid> productIds);
}
