namespace ETicaret.Business.DTOs;

public record CouponResultDto(
    bool IsValid,
    string Message,
    decimal DiscountAmount
);
