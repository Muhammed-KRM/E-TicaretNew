namespace ETicaret.Data.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    
    // Ürün ismi değişirse siparişteki kayıt etkilenmemesi için
    public string ProductTitle { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    // Navigation Properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
