using ETicaret.Data.Enums;

namespace ETicaret.Business.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
}

public class OrderStatusChangedEvent
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
}

public class OrderShippedEvent
{
    public Guid OrderId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
}

public class RefundApprovedEvent
{
    public Guid OrderId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
}
