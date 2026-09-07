using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using ETicaret.Business.Services;
using ETicaret.Business.DTOs;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.UnitTests.Business.Services;

public class WishlistManagerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<WishlistManager>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly WishlistManager _wishlistManager;

    public WishlistManagerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_Wishlist_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<WishlistManager>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _wishlistManager = new WishlistManager(_context, _mockLogger.Object, _memoryCache);
    }

    [Fact]
    public async Task AddToWishlistAsync_ShouldAddProduct_WhenProductExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = 1;
        
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            Slug = "test-category"
        };
        await _context.Categories.AddAsync(category);
        
        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            Price = 100,
            StockQuantity = 10,
            CategoryId = categoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _wishlistManager.AddToWishlistAsync(userId, productId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("favorilere eklendi");
        
        var wishlistItem = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        wishlistItem.Should().NotBeNull();
    }

    [Fact]
    public async Task AddToWishlistAsync_ShouldReturnError_WhenProductNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentProductId = Guid.NewGuid();

        // Act
        var result = await _wishlistManager.AddToWishlistAsync(userId, nonExistentProductId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task AddToWishlistAsync_ShouldReturnError_WhenProductAlreadyInWishlist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = 1;

        var category = new Category { Id = categoryId, Name = "Test", Slug = "test" };
        await _context.Categories.AddAsync(category);

        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            Price = 100,
            StockQuantity = 10,
            CategoryId = categoryId,
            IsActive = true
        };
        await _context.Products.AddAsync(product);

        var wishlist = new Wishlist
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Wishlists.AddAsync(wishlist);
        await _context.SaveChangesAsync();

        // Act
        var result = await _wishlistManager.AddToWishlistAsync(userId, productId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("zaten favorilerinizde");
    }

    [Fact]
    public async Task RemoveFromWishlistAsync_ShouldRemoveProduct_WhenProductExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = 1;

        var category = new Category { Id = categoryId, Name = "Test", Slug = "test" };
        await _context.Categories.AddAsync(category);

        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            Price = 100,
            StockQuantity = 10,
            CategoryId = categoryId,
            IsActive = true
        };
        await _context.Products.AddAsync(product);

        var wishlist = new Wishlist
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Wishlists.AddAsync(wishlist);
        await _context.SaveChangesAsync();

        // Act
        var result = await _wishlistManager.RemoveFromWishlistAsync(userId, productId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("favorilerden çıkarıldı");
        
        var wishlistItem = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        wishlistItem.Should().BeNull();
    }

    [Fact]
    public async Task GetUserWishlistAsync_ShouldReturnAllItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = 1;

        var category = new Category { Id = categoryId, Name = "Test", Slug = "test" };
        await _context.Categories.AddAsync(category);

        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Product 1",
            Price = 100,
            StockQuantity = 10,
            CategoryId = categoryId,
            IsActive = true
        };
        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Product 2",
            Price = 200,
            StockQuantity = 5,
            CategoryId = categoryId,
            IsActive = true
        };
        await _context.Products.AddRangeAsync(product1, product2);

        var wishlist1 = new Wishlist { UserId = userId, ProductId = product1.Id, CreatedAt = DateTime.UtcNow };
        var wishlist2 = new Wishlist { UserId = userId, ProductId = product2.Id, CreatedAt = DateTime.UtcNow };
        await _context.Wishlists.AddRangeAsync(wishlist1, wishlist2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _wishlistManager.GetUserWishlistAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(w => w.Title == "Product 1");
        result.Should().Contain(w => w.Title == "Product 2");
    }

    [Fact]
    public async Task IsInWishlistAsync_ShouldReturnTrue_WhenProductInWishlist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var wishlist = new Wishlist
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Wishlists.AddAsync(wishlist);
        await _context.SaveChangesAsync();

        // Act
        var result = await _wishlistManager.IsInWishlistAsync(userId, productId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetWishlistCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var wishlist1 = new Wishlist { UserId = userId, ProductId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        var wishlist2 = new Wishlist { UserId = userId, ProductId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        var wishlist3 = new Wishlist { UserId = userId, ProductId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        
        await _context.Wishlists.AddRangeAsync(wishlist1, wishlist2, wishlist3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _wishlistManager.GetWishlistCountAsync(userId);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task ClearWishlistAsync_ShouldRemoveAllItems()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var wishlist1 = new Wishlist { UserId = userId, ProductId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        var wishlist2 = new Wishlist { UserId = userId, ProductId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        
        await _context.Wishlists.AddRangeAsync(wishlist1, wishlist2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _wishlistManager.ClearWishlistAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.CurrentCount.Should().Be(0);
        
        var wishlistItems = await _context.Wishlists.Where(w => w.UserId == userId).ToListAsync();
        wishlistItems.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
