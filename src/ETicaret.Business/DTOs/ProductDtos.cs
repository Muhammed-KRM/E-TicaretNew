using System.ComponentModel.DataAnnotations;
using ETicaret.Data.Enums;

namespace ETicaret.Business.DTOs;

public record ProductDto(
    Guid Id,
    Guid SellerId,
    string SellerName,
    int CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Description,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    string? Brand,
    bool IsActive,
    bool IsFeatured,
    double AverageRating,
    int ReviewCount,
    int SalesCount,
    List<string> ImageUrls,
    DateTime CreatedAt
);

public class ProductCreateDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(5000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 1000000)]
    public decimal Price { get; set; }
    
    public decimal? DiscountedPrice { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    
    public string? SKU { get; set; }
    public string? Brand { get; set; }
    public decimal? Weight { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}

public class ProductUpdateDto : ProductCreateDto
{
    public bool IsActive { get; set; }
}
