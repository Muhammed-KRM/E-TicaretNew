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
