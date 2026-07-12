using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface ICouponService
{
    Task<CouponResultDto> ValidateCouponAsync(string code, decimal cartTotal);
    Task UseCouponAsync(string code, Guid userId, Guid orderId);
}
