using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ETicaret.Business.DTOs;
using ETicaret.Business.Exceptions;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using ETicaret.Data.Enums;

namespace ETicaret.Business.Services;

/// <summary>
/// Refund (İade) yönetimi servisi
/// 14 günlük yasal iade hakkı yönetimi
/// </summary>
public class RefundManager : IRefundService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RefundManager> _logger;
    private readonly INotificationService _notificationService;
    
    private const int REFUND_PERIOD_DAYS = 14; // Yasal iade süresi

    public RefundManager(
        AppDbContext context,
        ILogger<RefundManager> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<List<RefundDto>> GetUserRefundsAsync(Guid userId)
    {
        _logger.LogInformation("Getting refunds for user: {UserId}", userId);
        
        try
        {
            var refunds = await _context.Refunds
                .AsNoTracking()
                .Include(r => r.Order)
                .Include(r => r.User)
                .Include(r => r.ProcessedByUser)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RefundDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    OrderNumber = r.Order.OrderNumber,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    Reason = r.Reason,
                    Notes = r.Notes,
                    Status = r.Status,
                    AdminNotes = r.AdminNotes,
                    RefundAmount = r.RefundAmount,
                    CreatedAt = r.CreatedAt,
                    ProcessedAt = r.ProcessedAt,
                    ProcessedByName = r.ProcessedByUser != null ? r.ProcessedByUser.FullName : null
                })
                .ToListAsync();
            
            return refunds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refunds for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<RefundDto?> GetRefundByIdAsync(Guid refundId)
    {
        try
        {
            var refund = await _context.Refunds
                .AsNoTracking()
                .Include(r => r.Order)
                .Include(r => r.User)
                .Include(r => r.ProcessedByUser)
                .Where(r => r.Id == refundId)
                .Select(r => new RefundDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    OrderNumber = r.Order.OrderNumber,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    Reason = r.Reason,
                    Notes = r.Notes,
                    Status = r.Status,
                    AdminNotes = r.AdminNotes,
                    RefundAmount = r.RefundAmount,
                    CreatedAt = r.CreatedAt,
                    ProcessedAt = r.ProcessedAt,
                    ProcessedByName = r.ProcessedByUser != null ? r.ProcessedByUser.FullName : null
                })
                .FirstOrDefaultAsync();
            
            return refund;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refund by id: {RefundId}", refundId);
            throw;
        }
    }

    public async Task<RefundResult> CreateRefundAsync(Guid userId, CreateRefundRequest request)
    {
        _logger.LogInformation("Creating refund request for order {OrderId} by user {UserId}", request.OrderId, userId);
        
        try
        {
            // Sipariş kontrolü
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);
            
            if (order == null)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = "Sipariş bulunamadı"
                };
            }

            // Kullanıcı kontrolü
            if (order.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to create refund for order {OrderId} belonging to another user", userId, request.OrderId);
                return new RefundResult
                {
                    Success = false,
                    Message = "Bu sipariş size ait değil"
                };
            }

            // Sipariş durumu kontrolü - Sadece teslim edilmiş siparişler için iade
            if (order.Status != OrderStatus.Delivered)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = "Sadece teslim edilmiş siparişler için iade talebi oluşturulabilir"
                };
            }

            // 14 günlük süre kontrolü
            if (order.DeliveredAt == null)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = "Teslimat tarihi bulunamadı"
                };
            }

            var daysSinceDelivery = (DateTime.UtcNow - order.DeliveredAt.Value).TotalDays;
            if (daysSinceDelivery > REFUND_PERIOD_DAYS)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = $"İade süresi ({REFUND_PERIOD_DAYS} gün) dolmuştur"
                };
            }

            // Daha önce iade talebi oluşturulmuş mu kontrol et
            var existingRefund = await _context.Refunds
                .AnyAsync(r => r.OrderId == request.OrderId);
            
            if (existingRefund)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = "Bu sipariş için zaten bir iade talebi mevcut"
                };
            }

            // İade talebi oluştur
            var refund = new Refund
            {
                OrderId = request.OrderId,
                UserId = userId,
                Reason = request.Reason,
                Notes = request.Notes,
                Status = RefundStatus.Pending,
                RefundAmount = order.TotalPrice, // Başlangıçta tam tutar
                CreatedAt = DateTime.UtcNow
            };

            await _context.Refunds.AddAsync(refund);
            
            // Sipariş durumunu güncelle
            order.Status = OrderStatus.ReturnRequested;
            order.ReturnRequestedAt = DateTime.UtcNow;
            order.ReturnReason = request.Reason;
            
            await _context.SaveChangesAsync();

            // Admin'e bildirim gönder (basit versiyon)
            try
            {
                await _notificationService.CreateAsync(
                    userId, // Temporary - Admin notification gerekir
                    "RefundRequest",
                    "Yeni İade Talebi",
                    $"Sipariş #{order.OrderNumber} için iade talebi oluşturuldu."
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send admin notification for refund {RefundId}", refund.Id);
            }

            _logger.LogInformation("Refund request created successfully. RefundId: {RefundId}", refund.Id);
            
            return new RefundResult
            {
                Success = true,
                Message = "İade talebiniz başarıyla oluşturuldu",
                RefundId = refund.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating refund for order {OrderId}", request.OrderId);
            return new RefundResult
            {
                Success = false,
                Message = "İade talebi oluşturulurken bir hata oluştu"
            };
        }
    }

    public async Task<RefundResult> ApproveRefundAsync(Guid refundId, Guid adminId, ApproveRefundRequest request)
    {
        _logger.LogInformation("Approving refund {RefundId} by admin {AdminId}", refundId, adminId);
        
        try
        {
            var refund = await _context.Refunds
                .Include(r => r.Order)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == refundId);
            
            if (refund == null)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = "İade talebi bulunamadı"
                };
            }

            if (refund.Status != RefundStatus.Pending)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = $"Bu iade talebi zaten işlenmiş (Durum: {refund.Status})"
                };
            }

            // İade talebini onayla
            refund.Status = RefundStatus.Approved;
            refund.AdminNotes = request.AdminNotes;
            refund.RefundAmount = request.RefundAmount;
            refund.ProcessedBy = adminId;
            refund.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Kullanıcıya bildirim gönder
            try
            {
                await _notificationService.CreateAsync(
                    refund.UserId,
                    "RefundApproved",
                    "İade Talebiniz Onaylandı",
                    $"Sipariş #{refund.Order.OrderNumber} için iade talebiniz onaylandı."
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send user notification for refund {RefundId}", refundId);
            }

            _logger.LogInformation("Refund {RefundId} approved successfully", refundId);
            
            return new RefundResult
            {
                Success = true,
                Message = "İade talebi onaylandı",
                RefundId = refund.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving refund {RefundId}", refundId);
            return new RefundResult
            {
                Success = false,
                Message = "İade talebi onaylanırken bir hata oluştu"
            };
        }
    }

    public async Task<RefundResult> RejectRefundAsync(Guid refundId, Guid adminId, RejectRefundRequest request)
    {
        _logger.LogInformation("Rejecting refund {RefundId} by admin {AdminId}", refundId, adminId);
        
        try
        {
            var refund = await _context.Refunds
                .Include(r => r.Order)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == refundId);
            
            if (refund == null)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = "İade talebi bulunamadı"
                };
            }

            if (refund.Status != RefundStatus.Pending)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = $"Bu iade talebi zaten işlenmiş (Durum: {refund.Status})"
                };
            }

            // İade talebini reddet
            refund.Status = RefundStatus.Rejected;
            refund.AdminNotes = request.Reason;
            refund.ProcessedBy = adminId;
            refund.ProcessedAt = DateTime.UtcNow;

            // Sipariş durumunu geri al
            refund.Order.Status = OrderStatus.Delivered;
            refund.Order.AdminReturnNote = request.Reason;

            await _context.SaveChangesAsync();

            // Kullanıcıya bildirim gönder
            try
            {
                await _notificationService.CreateAsync(
                    refund.UserId,
                    "RefundRejected",
                    "İade Talebiniz Reddedildi",
                    $"Sipariş #{refund.Order.OrderNumber} için iade talebiniz reddedildi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send user notification for refund {RefundId}", refundId);
            }

            _logger.LogInformation("Refund {RefundId} rejected successfully", refundId);
            
            return new RefundResult
            {
                Success = true,
                Message = "İade talebi reddedildi",
                RefundId = refund.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting refund {RefundId}", refundId);
            return new RefundResult
            {
                Success = false,
                Message = "İade talebi reddedilirken bir hata oluştu"
            };
        }
    }

    public async Task<RefundResult> CompleteRefundAsync(Guid refundId, Guid adminId, CompleteRefundRequest request)
    {
        _logger.LogInformation("Completing refund {RefundId} by admin {AdminId}", refundId, adminId);
        
        try
        {
            var refund = await _context.Refunds
                .Include(r => r.Order)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == refundId);
            
            if (refund == null)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = "İade talebi bulunamadı"
                };
            }

            if (refund.Status != RefundStatus.Approved)
            {
                return new RefundResult
                {
                    Success = false,
                    Message = "Sadece onaylanmış iade talepleri tamamlanabilir"
                };
            }

            // İade işlemini tamamla
            refund.Status = RefundStatus.Refunded;
            if (!string.IsNullOrEmpty(request.Notes))
            {
                refund.AdminNotes = (refund.AdminNotes ?? "") + "\n" + request.Notes;
            }

            // Sipariş durumunu güncelle
            refund.Order.Status = OrderStatus.Refunded;
            refund.Order.RefundedAt = DateTime.UtcNow;
            refund.Order.RefundAmount = refund.RefundAmount;

            await _context.SaveChangesAsync();

            // Kullanıcıya bildirim gönder
            try
            {
                await _notificationService.CreateAsync(
                    refund.UserId,
                    "RefundCompleted",
                    "Para İadesi Tamamlandı",
                    $"Sipariş #{refund.Order.OrderNumber} için {refund.RefundAmount:C} tutarında para iadesi tamamlandı."
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send user notification for refund {RefundId}", refundId);
            }

            _logger.LogInformation("Refund {RefundId} completed successfully", refundId);
            
            return new RefundResult
            {
                Success = true,
                Message = "Para iadesi tamamlandı",
                RefundId = refund.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing refund {RefundId}", refundId);
            return new RefundResult
            {
                Success = false,
                Message = "Para iadesi tamamlanırken bir hata oluştu"
            };
        }
    }

    public async Task<PagedResult<RefundDto>> GetRefundsAsync(RefundFilterDto filter)
    {
        try
        {
            var query = _context.Refunds
                .AsNoTracking()
                .Include(r => r.Order)
                .Include(r => r.User)
                .Include(r => r.ProcessedByUser)
                .AsQueryable();

            // Filtreleme
            if (filter.UserId.HasValue)
            {
                query = query.Where(r => r.UserId == filter.UserId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(r => r.Status == filter.Status);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= filter.EndDate.Value);
            }

            // Toplam kayıt sayısı
            var totalCount = await query.CountAsync();

            // Sayfalama
            var refunds = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new RefundDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    OrderNumber = r.Order.OrderNumber,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    Reason = r.Reason,
                    Notes = r.Notes,
                    Status = r.Status,
                    AdminNotes = r.AdminNotes,
                    RefundAmount = r.RefundAmount,
                    CreatedAt = r.CreatedAt,
                    ProcessedAt = r.ProcessedAt,
                    ProcessedByName = r.ProcessedByUser != null ? r.ProcessedByUser.FullName : null
                })
                .ToListAsync();

            return new PagedResult<RefundDto>
            {
                Items = refunds,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refunds with filter");
            throw;
        }
    }

    public async Task<bool> CanCreateRefundAsync(Guid orderId, Guid userId)
    {
        try
        {
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            
            if (order == null) return false;
            if (order.Status != OrderStatus.Delivered) return false;
            if (order.DeliveredAt == null) return false;

            var daysSinceDelivery = (DateTime.UtcNow - order.DeliveredAt.Value).TotalDays;
            if (daysSinceDelivery > REFUND_PERIOD_DAYS) return false;

            var existingRefund = await _context.Refunds.AnyAsync(r => r.OrderId == orderId);
            if (existingRefund) return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if refund can be created for order {OrderId}", orderId);
            return false;
        }
    }

    public async Task<RefundDto?> GetRefundByOrderIdAsync(Guid orderId)
    {
        try
        {
            return await _context.Refunds
                .AsNoTracking()
                .Include(r => r.Order)
                .Include(r => r.User)
                .Include(r => r.ProcessedByUser)
                .Where(r => r.OrderId == orderId)
                .Select(r => new RefundDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    OrderNumber = r.Order.OrderNumber,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    Reason = r.Reason,
                    Notes = r.Notes,
                    Status = r.Status,
                    AdminNotes = r.AdminNotes,
                    RefundAmount = r.RefundAmount,
                    CreatedAt = r.CreatedAt,
                    ProcessedAt = r.ProcessedAt,
                    ProcessedByName = r.ProcessedByUser != null ? r.ProcessedByUser.FullName : null
                })
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refund by order id: {OrderId}", orderId);
            throw;
        }
    }
}
