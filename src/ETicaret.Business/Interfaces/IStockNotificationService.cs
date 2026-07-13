using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

/// <summary>
/// Stok bildirimi işlemleri için servis interface'i
/// </summary>
public interface IStockNotificationService
{
    /// <summary>
    /// Kullanıcıyı stok bildirimine abone eder
    /// Ürün stokta yoksa email kaydeder
    /// Ürün stoğa girdiğinde otomatik bildirim gönderilir
    /// </summary>
    /// <param name="productId">Ürün ID'si</param>
    /// <param name="request">Email bilgisi</param>
    /// <returns>İşlem sonucu</returns>
    Task<StockNotificationResult> SubscribeAsync(Guid productId, SubscribeStockNotificationRequest request);
    
    /// <summary>
    /// Ürün stoğa girdiğinde bekleyen tüm bildirimleri gönderir
    /// Worker service tarafından tetiklenir
    /// </summary>
    /// <param name="productId">Ürün ID'si</param>
    /// <returns>Gönderim sonuçları</returns>
    Task<SendStockNotificationsResult> SendNotificationsForProductAsync(Guid productId);
    
    /// <summary>
    /// Belirli bir email için bekleyen bildirimleri getirir (Admin)
    /// </summary>
    /// <param name="email">Email adresi</param>
    /// <returns>Bildirim listesi</returns>
    Task<List<StockNotificationDto>> GetNotificationsByEmailAsync(string email);
    
    /// <summary>
    /// Ürün için bekleyen tüm bildirimleri getirir (Admin)
    /// </summary>
    /// <param name="productId">Ürün ID'si</param>
    /// <returns>Bildirim listesi</returns>
    Task<List<StockNotificationDto>> GetNotificationsByProductAsync(Guid productId);
    
    /// <summary>
    /// Belirli bir bildirimi manuel olarak gönderir (Admin)
    /// </summary>
    /// <param name="notificationId">Bildirim ID'si</param>
    /// <returns>İşlem sonucu</returns>
    Task<StockNotificationResult> SendSingleNotificationAsync(Guid notificationId);
    
    /// <summary>
    /// Kullanıcı daha önce bu ürün için bildirim talebinde bulunmuş mu kontrol eder
    /// </summary>
    /// <param name="productId">Ürün ID'si</param>
    /// <param name="email">Email adresi</param>
    /// <returns>True ise bildirim talebi var</returns>
    Task<bool> HasPendingNotificationAsync(Guid productId, string email);
    
    /// <summary>
    /// Gönderilmemiş tüm bildirimleri getirir (Admin)
    /// </summary>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt</param>
    /// <returns>Sayfalanmış bildirim listesi</returns>
    Task<PagedResult<StockNotificationDto>> GetPendingNotificationsAsync(int page = 1, int pageSize = 50);
    
    /// <summary>
    /// Eski gönderilmiş bildirimleri temizler (cleanup job için)
    /// </summary>
    /// <param name="olderThanDays">Kaç gün öncesinden eski kayıtlar silinsin</param>
    /// <returns>Silinen kayıt sayısı</returns>
    Task<int> CleanupOldNotificationsAsync(int olderThanDays = 90);
}
