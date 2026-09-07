using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ETicaret.Business.Services;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.UnitTests.Business.Services;

public class StockNotificationManagerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<StockNotificationManager>> _mockLogger;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly StockNotificationManager _manager;

    public StockNotificationManagerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_StockNotification_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<StockNotificationManager>>();
        _mockEmailService = new Mock<IEmailService>();
        _manager = new StockNotificationManager(_context, _mockLogger.Object, _mockEmailService.Object);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldCreateNotification_WhenProductOutOfStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = 1;

        var category = new Category { Id = categoryId, Name = "Test", Slug = "test" };
        await _context.Categories.AddAsync(category);

        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            Price = 100,
            StockQuantity = 0, // Out of stock
            CategoryId = categoryId,
            IsActive = true
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new SubscribeStockNotificationRequest
        {
            Email = "customer@test.com"
        };

        // Act
        var result = await _manager.SubscribeAsync(productId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("stoğa girdiğinde");
        
        var notification = await _context.StockNotifications
            .FirstOrDefaultAsync(sn => sn.ProductId == productId && sn.Email == request.Email);
        notification.Should().NotBeNull();
        notification!.IsNotified.Should().BeFalse();
    }

    [Fact]
    public async Task SubscribeAsync_ShouldReturnError_WhenProductInStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = 1;

        var category = new Category { Id = categoryId, Name = "Test", Slug = "test" };
        await _context.Categories.AddAsync(category);

        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            Price = 100,
            StockQuantity = 10, // In stock
            CategoryId = categoryId,
            IsActive = true
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new SubscribeStockNotificationRequest
        {
            Email = "customer@test.com"
        };

        // Act
        var result = await _manager.SubscribeAsync(productId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("stokta mevcut");
    }

    [Fact]
    public async Task SubscribeAsync_ShouldReturnError_WhenAlreadySubscribed()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = 1;

        var category = new Category { Id = categoryId, Name = "Test", Slug = "test" };
        await _context.Categories.AddAsync(category);

        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            Price = 100,
            StockQuantity = 0,
            CategoryId = categoryId,
            IsActive = true
        };
        await _context.Products.AddAsync(product);

        var existingNotification = new StockNotification
        {
            ProductId = productId,
            Email = "customer@test.com",
            IsNotified = false,
            CreatedAt = DateTime.UtcNow
        };
        await _context.StockNotifications.AddAsync(existingNotification);
        await _context.SaveChangesAsync();

        var request = new SubscribeStockNotificationRequest
        {
            Email = "customer@test.com"
        };

        // Act
        var result = await _manager.SubscribeAsync(productId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("zaten bildirim talebiniz mevcut");
    }

    [Fact]
    public async Task SendNotificationsForProductAsync_ShouldSendEmails_WhenProductInStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = 1;

        var category = new Category { Id = categoryId, Name = "Test", Slug = "test" };
        await _context.Categories.AddAsync(category);

        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            Price = 100,
            StockQuantity = 10, // Back in stock
            CategoryId = categoryId,
            IsActive = true
        };
        await _context.Products.AddAsync(product);

        var notification1 = new StockNotification
        {
            ProductId = productId,
            Email = "customer1@test.com",
            IsNotified = false,
            CreatedAt = DateTime.UtcNow
        };
        var notification2 = new StockNotification
        {
            ProductId = productId,
            Email = "customer2@test.com",
            IsNotified = false,
            CreatedAt = DateTime.UtcNow
        };
        await _context.StockNotifications.AddRangeAsync(notification1, notification2);
        await _context.SaveChangesAsync();

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _manager.SendNotificationsForProductAsync(productId);

        // Assert
        result.Success.Should().BeTrue();
        result.SentCount.Should().Be(2);
        result.FailedCount.Should().Be(0);
        
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        
        var notifications = await _context.StockNotifications.Where(sn => sn.ProductId == productId).ToListAsync();
        notifications.Should().AllSatisfy(n => n.IsNotified.Should().BeTrue());
    }

    [Fact]
    public async Task SendNotificationsForProductAsync_ShouldReturnError_WhenProductOutOfStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = 1;

        var category = new Category { Id = categoryId, Name = "Test", Slug = "test" };
        await _context.Categories.AddAsync(category);

        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            Price = 100,
            StockQuantity = 0, // Still out of stock
            CategoryId = categoryId,
            IsActive = true
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _manager.SendNotificationsForProductAsync(productId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("stokta değil");
    }

    [Fact]
    public async Task GetNotificationsByEmailAsync_ShouldReturnAllNotifications()
    {
        // Arrange
        var email = "customer@test.com";
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var categoryId = 1;

        var category = new Category { Id = categoryId, Name = "Test", Slug = "test" };
        await _context.Categories.AddAsync(category);

        var product1 = new Product
        {
            Id = productId1,
            Title = "Product 1",
            Price = 100,
            StockQuantity = 0,
            CategoryId = categoryId,
            IsActive = true
        };
        var product2 = new Product
        {
            Id = productId2,
            Title = "Product 2",
            Price = 200,
            StockQuantity = 0,
            CategoryId = categoryId,
            IsActive = true
        };
        await _context.Products.AddRangeAsync(product1, product2);

        var notification1 = new StockNotification
        {
            ProductId = productId1,
            Email = email,
            IsNotified = false,
            CreatedAt = DateTime.UtcNow
        };
        var notification2 = new StockNotification
        {
            ProductId = productId2,
            Email = email,
            IsNotified = true,
            NotifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        await _context.StockNotifications.AddRangeAsync(notification1, notification2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _manager.GetNotificationsByEmailAsync(email);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(n => n.ProductName == "Product 1");
        result.Should().Contain(n => n.ProductName == "Product 2");
    }

    [Fact]
    public async Task HasPendingNotificationAsync_ShouldReturnTrue_WhenPendingNotificationExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var email = "customer@test.com";

        var notification = new StockNotification
        {
            ProductId = productId,
            Email = email,
            IsNotified = false,
            CreatedAt = DateTime.UtcNow
        };
        await _context.StockNotifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _manager.HasPendingNotificationAsync(productId, email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPendingNotificationAsync_ShouldReturnFalse_WhenNotificationAlreadySent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var email = "customer@test.com";

        var notification = new StockNotification
        {
            ProductId = productId,
            Email = email,
            IsNotified = true, // Already sent
            NotifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        await _context.StockNotifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _manager.HasPendingNotificationAsync(productId, email);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
