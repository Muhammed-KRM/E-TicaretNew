namespace ETicaret.Business.DTOs;

public class UpdateShippingDto
{
    public string? ShippingCompany { get; set; }
    public string? TrackingNumber { get; set; }
}

public class UpdateOrderStatusDto
{
    public string NewStatus { get; set; } = string.Empty;  // "Preparing", "Shipped", "Delivered"
    public string? ShippingCompany { get; set; }
    public string? TrackingNumber { get; set; }
}
