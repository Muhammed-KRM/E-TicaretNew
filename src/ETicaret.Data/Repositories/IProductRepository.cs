using ETicaret.Data.Entities;

namespace ETicaret.Data.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugWithDetailsAsync(string slug);
    IQueryable<Product> GetActiveWithDetailsQueryable();
    Task<List<Product>> GetByCategoryAsync(int categoryId);
}
