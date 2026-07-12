namespace ETicaret.Business.Infrastructure.Search.Models;

public class ProductDocument
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public decimal Price { get; set; }
    public double AverageRating { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}
