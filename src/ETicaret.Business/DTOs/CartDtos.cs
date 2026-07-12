namespace ETicaret.Business.DTOs;

public record CartDto(
    List<CartItemDto> Items,
    decimal TotalPrice,
    int ItemCount
);

public record CartItemDto(
    Guid ProductId,
    string ProductTitle,
    string? ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal,
    bool InStock
);

public record AddToCartDto(
    Guid ProductId,
    int Quantity
);
