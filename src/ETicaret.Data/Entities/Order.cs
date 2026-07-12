using ETicaret.Data.Enums;

namespace ETicaret.Data.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public decimal SubTotal { get; set; }
    public decimal ShippingPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }

    public string? CouponCode { get; set; }

    public Guid? ShippingAddressId { get; set; }
    public Guid? BillingAddressId { get; set; }
    
    public string ShippingAddressSnapshot { get; set; } = string.Empty;
    public string? BillingAddressSnapshot { get; set; }
    
    public string? ShippingCompany { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? CustomerNote { get; set; }
    public string? TrackingNumber { get; set; }
    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
