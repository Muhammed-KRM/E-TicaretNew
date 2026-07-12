namespace ETicaret.Data.Entities;

using ETicaret.Data.Enums;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal ShippingPrice { get; set; }
    
    // JSON olarak adres snapshot'ı tutulur ki adres silinse bile sipariş kaydı bozulmasın
    public string ShippingAddressSnapshot { get; set; } = string.Empty;
    
    public string? PaymentTransactionId { get; set; }
    public string? Note { get; set; }
    public string? TrackingNumber { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
