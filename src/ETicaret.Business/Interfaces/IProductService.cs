using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface IProductService
{
    Task<ProductSearchResultDto> SearchAsync(ProductSearchFilterDto filters, CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto?> GetBySlugAsync(string slug);
    Task<List<ProductDto>> GetByCategoryAsync(int categoryId);
    Task<List<ProductDto>> GetFeaturedAsync();
    Task<ProductDto> CreateAsync(ProductCreateDto dto);
    Task<ProductDto> UpdateAsync(Guid id, ProductUpdateDto dto);
    Task DeleteAsync(Guid id);
}
