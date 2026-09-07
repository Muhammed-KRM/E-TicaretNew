using System;

namespace ETicaret.Business.DTOs;

public class AdminActivityDto
{
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public decimal WalletBalance { get; set; }
    public int ViolationCount { get; set; }
    public DateTime? BannedUntil { get; set; }
    public string? BanReason { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminProductDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int SalesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminReportDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalCustomers { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<OrdersByStatusDto> OrdersByStatus { get; set; } = new();
    public List<MonthlySalesDto> MonthlySales { get; set; } = new();
}

public class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal Revenue { get; set; }
}

public class OrdersByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlySalesDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
