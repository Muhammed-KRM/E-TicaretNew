using Microsoft.EntityFrameworkCore;
using ETicaret.Business.DTOs;
using ETicaret.Business.Exceptions;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using ETicaret.Data.Enums;
using ETicaret.Data.Repositories;

namespace ETicaret.Business.Services;

public class CouponManager : ICouponService
{
    private const string EC_VALIDATE = "CO-001";
    private const string EC_USE      = "CO-002";

    private readonly IRepository<Coupon> _couponRepo;
    private readonly AppDbContext _context;
    private readonly ILogService _logService;

    public CouponManager(IRepository<Coupon> couponRepo, AppDbContext context, ILogService logService)
    {
        _couponRepo = couponRepo;
        _context = context;
        _logService = logService;
    }

    public async Task<CouponResultDto> ValidateCouponAsync(string code, decimal cartTotal)
    {
        try
        {
            var coupon = (await _couponRepo.FindAsync(c => c.Code == code.ToUpperInvariant())).FirstOrDefault();
            if (coupon == null) return new CouponResultDto(false, "Geçersiz kupon kodu.", 0);

            if (!coupon.IsActive) return new CouponResultDto(false, "Bu kupon aktif değil.", 0);
            
            if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow) return new CouponResultDto(false, "Bu kuponun süresi dolmuş.", 0);
            
            if (coupon.MaxUsageCount.HasValue && coupon.CurrentUsageCount >= coupon.MaxUsageCount.Value) return new CouponResultDto(false, "Bu kuponun kullanım limiti dolmuş.", 0);
            
            if (coupon.MinOrderAmount.HasValue && cartTotal < coupon.MinOrderAmount.Value) return new CouponResultDto(false, $"Bu kuponu kullanmak için sepet tutarınız en az {coupon.MinOrderAmount.Value:C} olmalıdır.", 0);

            decimal discountAmount = coupon.DiscountType == DiscountType.FixedAmount 
                ? coupon.DiscountValue 
                : (cartTotal * coupon.DiscountValue / 100);

            if (discountAmount > cartTotal) discountAmount = cartTotal;

            return new CouponResultDto(true, "Kupon başarıyla uygulandı.", discountAmount);
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_VALIDATE, ex, new { code, cartTotal });
            return new CouponResultDto(false, "Kupon doğrulanırken bir hata oluştu.", 0);
        }
    }

    public async Task UseCouponAsync(string code, Guid userId, Guid orderId)
    {
        try
        {
            var coupon = (await _couponRepo.FindAsync(c => c.Code == code.ToUpperInvariant())).FirstOrDefault();
            if (coupon != null)
            {
                coupon.CurrentUsageCount++;
                _couponRepo.Update(coupon);
                await _couponRepo.SaveChangesAsync();

                // CouponUsage tablosuna ekle
                _context.Set<CouponUsage>().Add(new CouponUsage
                {
                    CouponId = coupon.Id,
                    UserId = userId,
                    OrderId = orderId
                });
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_USE, ex, new { code, userId, orderId }); throw; }
    }
}
