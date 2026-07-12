namespace ETicaret.Business.DTOs;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string PaymentStatus,
    DateTime CreatedAt,
    decimal SubTotal,
    decimal ShippingCost,
    decimal DiscountAmount,
    decimal TotalPrice,
    string? TrackingNumber,
    string? ReturnReason,
    string? CancellationReason,
    string? AdminReturnNote,
    DateTime? ReturnRequestedAt,
    DateTime? RefundedAt,
    decimal? RefundAmount,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    string? ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);

public record OrderCreateDto(
    Guid ShippingAddressId,
    Guid BillingAddressId,
    string? CouponCode,
    string? CustomerNote,
    string ReturnUrl,
    string? IpAddress,
    string? UserAgent
);

public record OrderResultDto(
    bool Success,
    Guid OrderId,
    string OrderNumber,
    string PaymentUrl
);

public class ReturnRequestDto
{
    public string ReturnReason { get; set; } = string.Empty;
}

public class ProcessReturnDto
{
    public string? AdminNote { get; set; }
}
