namespace ETicaret.Business.DTOs;

public class ProductSearchFilterDto
{
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public string? Brand { get; set; }
    public bool? InStock { get; set; }
    public bool? IsFeatured { get; set; }
    public double? MinRating { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } // "price_asc", "price_desc", "rating", "newest"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class ProductSearchResultDto
{
    public List<ProductDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
