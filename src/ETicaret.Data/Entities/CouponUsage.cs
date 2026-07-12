namespace ETicaret.Data.Entities;

public class CouponUsage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Coupon Coupon { get; set; } = null!;
    public User User { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
