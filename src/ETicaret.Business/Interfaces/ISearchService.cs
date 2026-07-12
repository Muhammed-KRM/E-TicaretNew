using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface ISearchService
{
    Task<ProductSearchResultDto> SearchAsync(ProductSearchFilterDto filters);
    Task IndexProductAsync(ProductDto product);
    Task DeleteProductIndexAsync(Guid productId);
}
