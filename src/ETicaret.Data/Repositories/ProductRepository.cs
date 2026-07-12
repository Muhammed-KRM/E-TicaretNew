using Microsoft.EntityFrameworkCore;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using ETicaret.Data.Enums;

namespace ETicaret.Data.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySlugWithDetailsAsync(string slug)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public IQueryable<Product> GetActiveWithDetailsQueryable()
    {
        return _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.StockQuantity > 0);
    }

    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        return await GetActiveWithDetailsQueryable()
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<List<Product>> GetFeaturedProductsAsync(int count)
    {
        return await GetActiveWithDetailsQueryable()
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
