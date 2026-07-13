using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ETicaret.Business.DTOs;
using ETicaret.Business.Exceptions;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;

namespace ETicaret.Business.Services;

/// <summary>
/// Wishlist (Favori Listesi) yönetimi servisi
/// </summary>
public class WishlistManager : IWishlistService
{
    private readonly AppDbContext _context;
    private readonly ILogger<WishlistManager> _logger;
    private readonly IMemoryCache _cache;

    public WishlistManager(
        AppDbContext context,
        ILogger<WishlistManager> logger,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<List<WishlistItemDto>> GetUserWishlistAsync(Guid userId)
    {
        _logger.LogInformation("Getting wishlist for user: {UserId}", userId);
        
        try
        {
            var wishlist = await _context.Wishlists
                .AsNoTracking()
                .Include(w => w.Product)
                    .ThenInclude(p => p.Images)
                .Include(w => w.Product)
                    .ThenInclude(p => p.Reviews)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WishlistItemDto
                {
                    Id = w.Id,
                    ProductId = w.ProductId,
                    Title = w.Product.Title,
                    ImageUrl = w.Product.Images.FirstOrDefault() != null ? w.Product.Images.FirstOrDefault()!.ImageUrl : "",
                    Price = w.Product.Price,
                    DiscountedPrice = w.Product.DiscountedPrice,
                    IsInStock = w.Product.StockQuantity > 0,
                    AddedAt = w.CreatedAt,
                    AverageRating = w.Product.Reviews.Any() ? w.Product.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = w.Product.Reviews.Count
                })
                .ToListAsync();
            
            _logger.LogInformation("Found {Count} items in wishlist for user: {UserId}", wishlist.Count, userId);
            return wishlist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<WishlistOperationResult> AddToWishlistAsync(Guid userId, Guid productId)
    {
        _logger.LogInformation("Adding product {ProductId} to wishlist for user {UserId}", productId, userId);
        
        try
        {
            // Ürün var mı kontrol et
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return new WishlistOperationResult
                {
                    Success = false,
                    Message = "Ürün bulunamadı"
                };
            }

            // Zaten favorilerde mi kontrol et
            var exists = await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
            
            if (exists)
            {
                return new WishlistOperationResult
                {
                    Success = false,
                    Message = "Bu ürün zaten favorilerinizde",
                    CurrentCount = await GetWishlistCountAsync(userId)
                };
            }

            // Favoriye ekle
            var wishlist = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Wishlists.AddAsync(wishlist);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            InvalidateCache(userId);

            var newCount = await GetWishlistCountAsync(userId);
            
            _logger.LogInformation("Product {ProductId} added to wishlist for user {UserId}", productId, userId);
            
            return new WishlistOperationResult
            {
                Success = true,
                Message = "Ürün favorilere eklendi",
                CurrentCount = newCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to wishlist for user {UserId}", productId, userId);
            return new WishlistOperationResult
            {
                Success = false,
                Message = "Ürün favorilere eklenirken bir hata oluştu"
            };
        }
    }

    public async Task<WishlistOperationResult> RemoveFromWishlistAsync(Guid userId, Guid productId)
    {
        _logger.LogInformation("Removing product {ProductId} from wishlist for user {UserId}", productId, userId);
        
        try
        {
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
            
            if (wishlistItem == null)
            {
                return new WishlistOperationResult
                {
                    Success = false,
                    Message = "Ürün favorilerinizde bulunamadı",
                    CurrentCount = await GetWishlistCountAsync(userId)
                };
            }

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            InvalidateCache(userId);

            var newCount = await GetWishlistCountAsync(userId);
            
            _logger.LogInformation("Product {ProductId} removed from wishlist for user {UserId}", productId, userId);
            
            return new WishlistOperationResult
            {
                Success = true,
                Message = "Ürün favorilerden çıkarıldı",
                CurrentCount = newCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product {ProductId} from wishlist for user {UserId}", productId, userId);
            return new WishlistOperationResult
            {
                Success = false,
                Message = "Ürün favorilerden çıkarılırken bir hata oluştu"
            };
        }
    }

    public async Task<bool> IsInWishlistAsync(Guid userId, Guid productId)
    {
        try
        {
            return await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if product {ProductId} is in wishlist for user {UserId}", productId, userId);
            return false;
        }
    }

    public async Task<int> GetWishlistCountAsync(Guid userId)
    {
        var cacheKey = $"wishlist_count_{userId}";
        
        if (_cache.TryGetValue(cacheKey, out int count))
        {
            return count;
        }

        try
        {
            count = await _context.Wishlists
                .CountAsync(w => w.UserId == userId);
            
            // Cache'e kaydet (5 dakika)
            _cache.Set(cacheKey, count, TimeSpan.FromMinutes(5));
            
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist count for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<WishlistOperationResult> ClearWishlistAsync(Guid userId)
    {
        _logger.LogInformation("Clearing wishlist for user {UserId}", userId);
        
        try
        {
            var items = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .ToListAsync();
            
            if (!items.Any())
            {
                return new WishlistOperationResult
                {
                    Success = true,
                    Message = "Favori listeniz zaten boş",
                    CurrentCount = 0
                };
            }

            _context.Wishlists.RemoveRange(items);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            InvalidateCache(userId);
            
            _logger.LogInformation("Cleared {Count} items from wishlist for user {UserId}", items.Count, userId);
            
            return new WishlistOperationResult
            {
                Success = true,
                Message = "Favori listeniz temizlendi",
                CurrentCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist for user {UserId}", userId);
            return new WishlistOperationResult
            {
                Success = false,
                Message = "Favori liste temizlenirken bir hata oluştu"
            };
        }
    }

    public async Task<WishlistOperationResult> AddMultipleToWishlistAsync(Guid userId, List<Guid> productIds)
    {
        _logger.LogInformation("Adding {Count} products to wishlist for user {UserId}", productIds.Count, userId);
        
        try
        {
            if (!productIds.Any())
            {
                return new WishlistOperationResult
                {
                    Success = false,
                    Message = "Ürün listesi boş"
                };
            }

            // Mevcut favorileri bul
            var existingProductIds = await _context.Wishlists
                .Where(w => w.UserId == userId && productIds.Contains(w.ProductId))
                .Select(w => w.ProductId)
                .ToListAsync();

            // Yeni eklenecekleri filtrele
            var newProductIds = productIds.Except(existingProductIds).ToList();
            
            if (!newProductIds.Any())
            {
                return new WishlistOperationResult
                {
                    Success = false,
                    Message = "Tüm ürünler zaten favorilerinizde",
                    CurrentCount = await GetWishlistCountAsync(userId)
                };
            }

            // Ürünlerin var olduğunu kontrol et
            var validProductIds = await _context.Products
                .Where(p => newProductIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            // Toplu ekleme
            var wishlistItems = validProductIds.Select(productId => new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.Wishlists.AddRangeAsync(wishlistItems);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            InvalidateCache(userId);

            var newCount = await GetWishlistCountAsync(userId);
            
            _logger.LogInformation("Added {Count} products to wishlist for user {UserId}", wishlistItems.Count, userId);
            
            return new WishlistOperationResult
            {
                Success = true,
                Message = $"{wishlistItems.Count} ürün favorilere eklendi",
                CurrentCount = newCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding multiple products to wishlist for user {UserId}", userId);
            return new WishlistOperationResult
            {
                Success = false,
                Message = "Ürünler favorilere eklenirken bir hata oluştu"
            };
        }
    }

    private void InvalidateCache(Guid userId)
    {
        var cacheKey = $"wishlist_count_{userId}";
        _cache.Remove(cacheKey);
    }
}
