using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ETicaret.Business.Services;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using ETicaret.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.UnitTests.Business.Services;

public class RefundManagerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<RefundManager>> _mockLogger;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly RefundManager _refundManager;

    public RefundManagerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_Refund_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<RefundManager>>();
        _mockNotificationService = new Mock<INotificationService>();
        _refundManager = new RefundManager(_context, _mockLogger.Object, _mockNotificationService.Object);
    }

    [Fact]
    public async Task CreateRefundAsync_ShouldCreateRefund_WhenOrderIsDelivered()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            FullName = "Test User",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        await _context.Users.AddAsync(user);

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = "ORD-001",
            Status = OrderStatus.Delivered,
            PaymentStatus = PaymentStatus.Paid,
            SubTotal = 100,
            ShippingPrice = 10,
            DiscountAmount = 0,
            TotalPrice = 110,
            DeliveredAt = DateTime.UtcNow.AddDays(-5),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var request = new CreateRefundRequest
        {
            OrderId = orderId,
            Reason = RefundReason.Defective,
            Notes = "Product is damaged"
        };

        // Act
        var result = await _refundManager.CreateRefundAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.RefundId.Should().NotBeNull();
        
        var refund = await _context.Refunds.FirstOrDefaultAsync(r => r.OrderId == orderId);
        refund.Should().NotBeNull();
        refund!.Status.Should().Be(RefundStatus.Pending);
        refund.Reason.Should().Be(RefundReason.Defective);
    }

    [Fact]
    public async Task CreateRefundAsync_ShouldReturnError_WhenOrderNotDelivered()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            FullName = "Test User",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        await _context.Users.AddAsync(user);

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = "ORD-002",
            Status = OrderStatus.Preparing, // Not delivered yet
            PaymentStatus = PaymentStatus.Paid,
            SubTotal = 100,
            ShippingPrice = 10,
            DiscountAmount = 0,
            TotalPrice = 110,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var request = new CreateRefundRequest
        {
            OrderId = orderId,
            Reason = RefundReason.Defective,
            Notes = "Product is damaged"
        };

        // Act
        var result = await _refundManager.CreateRefundAsync(userId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("teslim edilmiş");
    }

    [Fact]
    public async Task CreateRefundAsync_ShouldReturnError_WhenRefundPeriodExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            FullName = "Test User",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        await _context.Users.AddAsync(user);

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = "ORD-003",
            Status = OrderStatus.Delivered,
            PaymentStatus = PaymentStatus.Paid,
            SubTotal = 100,
            ShippingPrice = 10,
            DiscountAmount = 0,
            TotalPrice = 110,
            DeliveredAt = DateTime.UtcNow.AddDays(-20), // 20 days ago (> 14 days)
            CreatedAt = DateTime.UtcNow.AddDays(-25)
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var request = new CreateRefundRequest
        {
            OrderId = orderId,
            Reason = RefundReason.Defective,
            Notes = "Product is damaged"
        };

        // Act
        var result = await _refundManager.CreateRefundAsync(userId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("İade süresi");
    }

    [Fact]
    public async Task ApproveRefundAsync_ShouldApproveRefund_WhenRefundIsPending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var refundId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            FullName = "Test User",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        var admin = new User
        {
            Id = adminId,
            Email = "admin@test.com",
            FullName = "Admin User",
            Role = UserRole.Admin,
            PasswordHash = "hash"
        };
        await _context.Users.AddRangeAsync(user, admin);

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = "ORD-004",
            Status = OrderStatus.ReturnRequested,
            PaymentStatus = PaymentStatus.Paid,
            SubTotal = 100,
            ShippingPrice = 10,
            DiscountAmount = 0,
            TotalPrice = 110,
            DeliveredAt = DateTime.UtcNow.AddDays(-5),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        await _context.Orders.AddAsync(order);

        var refund = new Refund
        {
            Id = refundId,
            OrderId = orderId,
            UserId = userId,
            Reason = RefundReason.Defective,
            Notes = "Product is damaged",
            Status = RefundStatus.Pending,
            RefundAmount = 110,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Refunds.AddAsync(refund);
        await _context.SaveChangesAsync();

        var request = new ApproveRefundRequest
        {
            AdminNotes = "Refund approved",
            RefundAmount = 110
        };

        // Act
        var result = await _refundManager.ApproveRefundAsync(refundId, adminId, request);

        // Assert
        result.Success.Should().BeTrue();
        
        var updatedRefund = await _context.Refunds.FindAsync(refundId);
        updatedRefund!.Status.Should().Be(RefundStatus.Approved);
        updatedRefund.ProcessedBy.Should().Be(adminId);
        updatedRefund.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RejectRefundAsync_ShouldRejectRefund_WhenRefundIsPending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var refundId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            FullName = "Test User",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        var admin = new User
        {
            Id = adminId,
            Email = "admin@test.com",
            FullName = "Admin User",
            Role = UserRole.Admin,
            PasswordHash = "hash"
        };
        await _context.Users.AddRangeAsync(user, admin);

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = "ORD-005",
            Status = OrderStatus.ReturnRequested,
            PaymentStatus = PaymentStatus.Paid,
            SubTotal = 100,
            ShippingPrice = 10,
            DiscountAmount = 0,
            TotalPrice = 110,
            DeliveredAt = DateTime.UtcNow.AddDays(-5),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        await _context.Orders.AddAsync(order);

        var refund = new Refund
        {
            Id = refundId,
            OrderId = orderId,
            UserId = userId,
            Reason = RefundReason.ChangedMind,
            Notes = "Changed my mind",
            Status = RefundStatus.Pending,
            RefundAmount = 110,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Refunds.AddAsync(refund);
        await _context.SaveChangesAsync();

        var request = new RejectRefundRequest
        {
            Reason = "Product was opened and used"
        };

        // Act
        var result = await _refundManager.RejectRefundAsync(refundId, adminId, request);

        // Assert
        result.Success.Should().BeTrue();
        
        var updatedRefund = await _context.Refunds.FindAsync(refundId);
        updatedRefund!.Status.Should().Be(RefundStatus.Rejected);
        updatedRefund.ProcessedBy.Should().Be(adminId);
        updatedRefund.AdminNotes.Should().Contain("opened and used");
    }

    [Fact]
    public async Task GetUserRefundsAsync_ShouldReturnAllUserRefunds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            FullName = "Test User",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        await _context.Users.AddAsync(user);

        var order1 = new Order
        {
            Id = orderId1,
            UserId = userId,
            OrderNumber = "ORD-006",
            Status = OrderStatus.ReturnRequested,
            PaymentStatus = PaymentStatus.Paid,
            SubTotal = 100,
            ShippingPrice = 10,
            DiscountAmount = 0,
            TotalPrice = 110,
            CreatedAt = DateTime.UtcNow
        };
        var order2 = new Order
        {
            Id = orderId2,
            UserId = userId,
            OrderNumber = "ORD-007",
            Status = OrderStatus.Refunded,
            PaymentStatus = PaymentStatus.Refunded,
            SubTotal = 200,
            ShippingPrice = 15,
            DiscountAmount = 0,
            TotalPrice = 215,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Orders.AddRangeAsync(order1, order2);

        var refund1 = new Refund
        {
            OrderId = orderId1,
            UserId = userId,
            Reason = RefundReason.Defective,
            Notes = "Test 1",
            Status = RefundStatus.Pending,
            RefundAmount = 110,
            CreatedAt = DateTime.UtcNow
        };
        var refund2 = new Refund
        {
            OrderId = orderId2,
            UserId = userId,
            Reason = RefundReason.WrongItem,
            Notes = "Test 2",
            Status = RefundStatus.Refunded,
            RefundAmount = 215,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Refunds.AddRangeAsync(refund1, refund2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _refundManager.GetUserRefundsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.OrderNumber == "ORD-006");
        result.Should().Contain(r => r.OrderNumber == "ORD-007");
    }

    [Fact]
    public async Task CanCreateRefundAsync_ShouldReturnTrue_WhenConditionsAreMet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            FullName = "Test User",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        await _context.Users.AddAsync(user);

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = "ORD-008",
            Status = OrderStatus.Delivered,
            PaymentStatus = PaymentStatus.Paid,
            SubTotal = 100,
            ShippingPrice = 10,
            DiscountAmount = 0,
            TotalPrice = 110,
            DeliveredAt = DateTime.UtcNow.AddDays(-5),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _refundManager.CanCreateRefundAsync(orderId, userId);

        // Assert
        result.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
