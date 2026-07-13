using ETicaret.Business.DTOs;
using ETicaret.Data.Entities;

namespace ETicaret.Business.Interfaces;

/// <summary>
/// Refund (İade) işlemleri için servis interface'i
/// </summary>
public interface IRefundService
{
    /// <summary>
    /// Kullanıcının iade taleplerini getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>İade talepleri listesi</returns>
    Task<List<RefundDto>> GetUserRefundsAsync(Guid userId);
    
    /// <summary>
    /// İade talebi detayını getirir
    /// </summary>
    /// <param name="refundId">İade talebi ID'si</param>
    /// <returns>İade talebi detayı</returns>
    Task<RefundDto?> GetRefundByIdAsync(Guid refundId);
    
    /// <summary>
    /// Yeni iade talebi oluşturur
    /// 14 günlük iade süresi kontrolü yapar
    /// Sipariş durumu "Delivered" olmalı
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="request">İade talebi bilgileri</param>
    /// <returns>İşlem sonucu</returns>
    Task<RefundResult> CreateRefundAsync(Guid userId, CreateRefundRequest request);
    
    /// <summary>
    /// İade talebini onaylar (Admin)
    /// </summary>
    /// <param name="refundId">İade talebi ID'si</param>
    /// <param name="adminId">Admin kullanıcı ID'si</param>
    /// <param name="request">Onay bilgileri</param>
    /// <returns>İşlem sonucu</returns>
    Task<RefundResult> ApproveRefundAsync(Guid refundId, Guid adminId, ApproveRefundRequest request);
    
    /// <summary>
    /// İade talebini reddeder (Admin)
    /// </summary>
    /// <param name="refundId">İade talebi ID'si</param>
    /// <param name="adminId">Admin kullanıcı ID'si</param>
    /// <param name="request">Red nedeni</param>
    /// <returns>İşlem sonucu</returns>
    Task<RefundResult> RejectRefundAsync(Guid refundId, Guid adminId, RejectRefundRequest request);
    
    /// <summary>
    /// İade işlemini tamamlar - para iadesi yapıldı olarak işaretler (Admin)
    /// </summary>
    /// <param name="refundId">İade talebi ID'si</param>
    /// <param name="adminId">Admin kullanıcı ID'si</param>
    /// <param name="request">Tamamlama notları</param>
    /// <returns>İşlem sonucu</returns>
    Task<RefundResult> CompleteRefundAsync(Guid refundId, Guid adminId, CompleteRefundRequest request);
    
    /// <summary>
    /// Tüm iade taleplerini filtreli şekilde getirir (Admin)
    /// </summary>
    /// <param name="filter">Filtre parametreleri</param>
    /// <returns>Sayfalanmış iade listesi</returns>
    Task<PagedResult<RefundDto>> GetRefundsAsync(RefundFilterDto filter);
    
    /// <summary>
    /// Sipariş için iade oluşturulabilir mi kontrol eder
    /// - Sipariş durumu "Delivered" olmalı
    /// - 14 gün geçmemiş olmalı
    /// - Zaten iade talebi oluşturulmamış olmalı
    /// </summary>
    /// <param name="orderId">Sipariş ID'si</param>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>True ise iade oluşturulabilir</returns>
    Task<bool> CanCreateRefundAsync(Guid orderId, Guid userId);
    
    /// <summary>
    /// Sipariş için mevcut iade talebini getirir (varsa)
    /// </summary>
    /// <param name="orderId">Sipariş ID'si</param>
    /// <returns>İade talebi veya null</returns>
    Task<RefundDto?> GetRefundByOrderIdAsync(Guid orderId);
}
