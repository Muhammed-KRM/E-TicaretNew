using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;

namespace ETicaret.Business.Services;

/// <summary>
/// Stok bildirimi yönetimi servisi
/// Ürün stoğa girdiğinde otomatik email bildirimi
/// </summary>
public class StockNotificationManager : IStockNotificationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StockNotificationManager> _logger;
    private readonly IEmailService _emailService;

    public StockNotificationManager(
        AppDbContext context,
        ILogger<StockNotificationManager> logger,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<StockNotificationResult> SubscribeAsync(Guid productId, SubscribeStockNotificationRequest request)
    {
        _logger.LogInformation("Subscribing {Email} to stock notifications for product {ProductId}", request.Email, productId);
        
        try
        {
            // Ürün var mı kontrol et
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);
            
            if (product == null)
            {
                return new StockNotificationResult
                {
                    Success = false,
                    Message = "Ürün bulunamadı"
                };
            }

            // Ürün zaten stokta mı?
            if (product.StockQuantity > 0)
            {
                return new StockNotificationResult
                {
                    Success = false,
                    Message = "Bu ürün şu anda stokta mevcut"
                };
            }

            // Aynı email için bekleyen bildirim var mı?
            var existingNotification = await _context.StockNotifications
                .FirstOrDefaultAsync(sn => 
                    sn.ProductId == productId && 
                    sn.Email == request.Email && 
                    !sn.IsNotified);
            
            if (existingNotification != null)
            {
                return new StockNotificationResult
                {
                    Success = false,
                    Message = "Bu ürün için zaten bildirim talebiniz mevcut"
                };
            }

            // Yeni bildirim kaydı oluştur
            var notification = new StockNotification
            {
                ProductId = productId,
                Email = request.Email,
                IsNotified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.StockNotifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Stock notification subscription created for {Email} on product {ProductId}", request.Email, productId);
            
            return new StockNotificationResult
            {
                Success = true,
                Message = "Ürün stoğa girdiğinde email ile bilgilendirileceksiniz"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to stock notifications for product {ProductId}", productId);
            return new StockNotificationResult
            {
                Success = false,
                Message = "Bildirim kaydı oluşturulurken bir hata oluştu"
            };
        }
    }

    public async Task<SendStockNotificationsResult> SendNotificationsForProductAsync(Guid productId)
    {
        _logger.LogInformation("Sending stock notifications for product {ProductId}", productId);
        
        int sentCount = 0;
        int failedCount = 0;

        try
        {
            // Ürün bilgilerini al
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);
            
            if (product == null)
            {
                return new SendStockNotificationsResult
                {
                    Success = false,
                    Message = "Ürün bulunamadı",
                    SentCount = 0,
                    FailedCount = 0
                };
            }

            // Ürün stokta mı kontrol et
            if (product.StockQuantity <= 0)
            {
                return new SendStockNotificationsResult
                {
                    Success = false,
                    Message = "Ürün stokta değil",
                    SentCount = 0,
                    FailedCount = 0
                };
            }

            // Bekleyen bildirimleri al (max 1000)
            var notifications = await _context.StockNotifications
                .Where(sn => sn.ProductId == productId && !sn.IsNotified)
                .Take(1000)
                .ToListAsync();

            if (!notifications.Any())
            {
                return new SendStockNotificationsResult
                {
                    Success = true,
                    Message = "Gönderilecek bildirim yok",
                    SentCount = 0,
                    FailedCount = 0
                };
            }

            _logger.LogInformation("Found {Count} pending notifications for product {ProductId}", notifications.Count, productId);

            // Her bildirim için email gönder
            foreach (var notification in notifications)
            {
                try
                {
                    // Email gönder
                    var emailSubject = $"{product.Title} Stoğa Girdi!";
                    var emailBody = $@"
                        <h2>Beklediğiniz Ürün Stoğa Girdi!</h2>
                        <p><strong>{product.Title}</strong> artık stoklarımızda mevcut.</p>
                        <p>Fiyat: {product.Price:C}</p>
                        <p>Hemen sipariş vermek için sitemizi ziyaret edin!</p>
                    ";

                    await _emailService.SendEmailAsync(notification.Email, emailSubject, emailBody);

                    // Bildirim durumunu güncelle
                    notification.IsNotified = true;
                    notification.NotifiedAt = DateTime.UtcNow;
                    
                    sentCount++;
                    
                    _logger.LogDebug("Stock notification sent to {Email} for product {ProductId}", notification.Email, productId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send stock notification to {Email} for product {ProductId}", notification.Email, productId);
                    failedCount++;
                }
            }

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();

            _logger.LogInformation("Stock notifications sent. Success: {SentCount}, Failed: {FailedCount}", sentCount, failedCount);

            return new SendStockNotificationsResult
            {
                Success = true,
                Message = $"{sentCount} bildirim gönderildi, {failedCount} başarısız",
                SentCount = sentCount,
                FailedCount = failedCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending stock notifications for product {ProductId}", productId);
            return new SendStockNotificationsResult
            {
                Success = false,
                Message = "Bildirimler gönderilirken bir hata oluştu",
                SentCount = sentCount,
                FailedCount = failedCount
            };
        }
    }

    public async Task<List<StockNotificationDto>> GetNotificationsByEmailAsync(string email)
    {
        try
        {
            var notifications = await _context.StockNotifications
                .AsNoTracking()
                .Include(sn => sn.Product)
                    .ThenInclude(p => p.Images)
                .Where(sn => sn.Email == email)
                .OrderByDescending(sn => sn.CreatedAt)
                .Select(sn => new StockNotificationDto
                {
                    Id = sn.Id,
                    ProductId = sn.ProductId,
                    ProductName = sn.Product.Title,
                    ProductImageUrl = sn.Product.Images.FirstOrDefault() != null ? sn.Product.Images.FirstOrDefault()!.ImageUrl : "",
                    Email = sn.Email,
                    IsNotified = sn.IsNotified,
                    NotifiedAt = sn.NotifiedAt,
                    CreatedAt = sn.CreatedAt
                })
                .ToListAsync();

            return notifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications by email: {Email}", email);
            throw;
        }
    }

    public async Task<List<StockNotificationDto>> GetNotificationsByProductAsync(Guid productId)
    {
        try
        {
            var notifications = await _context.StockNotifications
                .AsNoTracking()
                .Include(sn => sn.Product)
                    .ThenInclude(p => p.Images)
                .Where(sn => sn.ProductId == productId)
                .OrderByDescending(sn => sn.CreatedAt)
                .Select(sn => new StockNotificationDto
                {
                    Id = sn.Id,
                    ProductId = sn.ProductId,
                    ProductName = sn.Product.Title,
                    ProductImageUrl = sn.Product.Images.FirstOrDefault() != null ? sn.Product.Images.FirstOrDefault()!.ImageUrl : "",
                    Email = sn.Email,
                    IsNotified = sn.IsNotified,
                    NotifiedAt = sn.NotifiedAt,
                    CreatedAt = sn.CreatedAt
                })
                .ToListAsync();

            return notifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications by product: {ProductId}", productId);
            throw;
        }
    }

    public async Task<StockNotificationResult> SendSingleNotificationAsync(Guid notificationId)
    {
        _logger.LogInformation("Sending single stock notification {NotificationId}", notificationId);
        
        try
        {
            var notification = await _context.StockNotifications
                .Include(sn => sn.Product)
                    .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(sn => sn.Id == notificationId);
            
            if (notification == null)
            {
                return new StockNotificationResult
                {
                    Success = false,
                    Message = "Bildirim bulunamadı"
                };
            }

            if (notification.IsNotified)
            {
                return new StockNotificationResult
                {
                    Success = false,
                    Message = "Bu bildirim zaten gönderilmiş"
                };
            }

            // Email gönder
            var emailSubject = $"{notification.Product.Title} Stoğa Girdi!";
            var emailBody = $@"
                <h2>Beklediğiniz Ürün Stoğa Girdi!</h2>
                <p><strong>{notification.Product.Title}</strong> artık stoklarımızda mevcut.</p>
                <p>Fiyat: {notification.Product.Price:C}</p>
                <p>Hemen sipariş vermek için sitemizi ziyaret edin!</p>
            ";

            await _emailService.SendEmailAsync(notification.Email, emailSubject, emailBody);

            // Bildirim durumunu güncelle
            notification.IsNotified = true;
            notification.NotifiedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Single stock notification sent successfully {NotificationId}", notificationId);

            return new StockNotificationResult
            {
                Success = true,
                Message = "Bildirim başarıyla gönderildi"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending single notification {NotificationId}", notificationId);
            return new StockNotificationResult
            {
                Success = false,
                Message = "Bildirim gönderilirken bir hata oluştu"
            };
        }
    }

    public async Task<bool> HasPendingNotificationAsync(Guid productId, string email)
    {
        try
        {
            return await _context.StockNotifications
                .AnyAsync(sn => 
                    sn.ProductId == productId && 
                    sn.Email == email && 
                    !sn.IsNotified);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking pending notification for product {ProductId}, email {Email}", productId, email);
            return false;
        }
    }

    public async Task<PagedResult<StockNotificationDto>> GetPendingNotificationsAsync(int page = 1, int pageSize = 50)
    {
        try
        {
            var query = _context.StockNotifications
                .AsNoTracking()
                .Include(sn => sn.Product)
                    .ThenInclude(p => p.Images)
                .Where(sn => !sn.IsNotified);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderBy(sn => sn.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sn => new StockNotificationDto
                {
                    Id = sn.Id,
                    ProductId = sn.ProductId,
                    ProductName = sn.Product.Title,
                    ProductImageUrl = sn.Product.Images.FirstOrDefault() != null ? sn.Product.Images.FirstOrDefault()!.ImageUrl : "",
                    Email = sn.Email,
                    IsNotified = sn.IsNotified,
                    NotifiedAt = sn.NotifiedAt,
                    CreatedAt = sn.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<StockNotificationDto>
            {
                Items = notifications,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending notifications");
            throw;
        }
    }

    public async Task<int> CleanupOldNotificationsAsync(int olderThanDays = 90)
    {
        _logger.LogInformation("Cleaning up old stock notifications older than {Days} days", olderThanDays);
        
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

            var oldNotifications = await _context.StockNotifications
                .Where(sn => sn.IsNotified && sn.NotifiedAt < cutoffDate)
                .ToListAsync();

            if (!oldNotifications.Any())
            {
                _logger.LogInformation("No old notifications to clean up");
                return 0;
            }

            _context.StockNotifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} old stock notifications", oldNotifications.Count);
            
            return oldNotifications.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old notifications");
            throw;
        }
    }
}
