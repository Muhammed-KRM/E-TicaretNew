using MassTransit;
using Microsoft.EntityFrameworkCore;
using ETicaret.Business.DTOs;
using ETicaret.Business.Events;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Enums;

namespace ETicaret.Business.Services;

public class AdminManager : IAdminService
{
    private const string EC_DASHBOARD      = "ADM-001";
    private const string EC_GETUSERS       = "ADM-002";
    private const string EC_SUSPENDUSER    = "ADM-003";
    private const string EC_ACTIVATEUSER   = "ADM-004";
    private const string EC_GETPRODUCTS    = "ADM-005";
    private const string EC_APPROVEPRODUCT = "ADM-006";
    private const string EC_REJECTPRODUCT  = "ADM-007";
    private const string EC_SUSPENDPRODUCT = "ADM-008";
    private const string EC_DELETEPRODUCT  = "ADM-009";
    private const string EC_GETORDERS      = "ADM-010";
    private const string EC_UPDATEORDER    = "ADM-011";

    private readonly AppDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogService _logService;

    public AdminManager(AppDbContext context, IPublishEndpoint publishEndpoint, ILogService logService)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logService = logService;
    }

    public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
    {
        try
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Preparing)
                .SumAsync(o => o.TotalPrice);

            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var pendingProducts = await _context.Products.CountAsync(p => !p.IsActive); // veya özel bir statü varsa
            var totalViolations = await _context.Users.SumAsync(u => u.ViolationCount);

            return new AdminDashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                ActiveUsers = activeUsers,
                PendingOrders = pendingOrders,
                PendingProducts = pendingProducts,
                TotalViolations = totalViolations
            };
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_DASHBOARD, ex); throw; }
    }

    public async Task<List<AdminUserDto>> GetAllUsersAsync(string? search = null, string? role = null, string? status = null)
    {
        try
        {
            var query = _context.Users.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
                
            if (!string.IsNullOrWhiteSpace(role))
            {
                if (Enum.TryParse<UserRole>(role, out var parsedRole))
                    query = query.Where(u => u.Role == parsedRole);
            }
            
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Active") query = query.Where(u => u.IsActive);
                else if (status == "Suspended") query = query.Where(u => !u.IsActive);
            }
            
            return await query.OrderByDescending(u => u.CreatedAt).Select(u => new AdminUserDto
            {
                Id = u.Id, 
                FullName = u.FullName, 
                Email = u.Email, 
                Phone = u.PhoneEncrypted ?? "—",
                Role = u.Role.ToString(),
                Status = u.IsActive ? "Active" : "Suspended", 
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                WalletBalance = u.WalletBalance,
                ViolationCount = u.ViolationCount,
                BannedUntil = u.BannedUntil,
                BanReason = u.BanReason,
                ProfileImageUrl = u.ProfileImageUrl,
                CreatedAt = u.CreatedAt
            }).ToListAsync();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETUSERS, ex, new { search, role, status }); throw; }
    }

    public async Task SuspendUserAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null) { user.IsActive = false; user.UpdatedAt = DateTime.UtcNow; await _context.SaveChangesAsync(); }
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_SUSPENDUSER, ex, userId); throw; }
    }

    public async Task ActivateUserAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null) { user.IsActive = true; user.UpdatedAt = DateTime.UtcNow; await _context.SaveChangesAsync(); }
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_ACTIVATEUSER, ex, userId); throw; }
    }

    public async Task<List<AdminProductDto>> GetAllProductsAsync(string? search = null, string? status = null)
    {
        try
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Title.Contains(search));
                
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Active") query = query.Where(p => p.IsActive);
                else if (status == "Inactive") query = query.Where(p => !p.IsActive);
            }
            
            return await query.OrderByDescending(p => p.CreatedAt).Select(p => new AdminProductDto
            {
                Id = p.Id, 
                Title = p.Title, 
                Category = p.Category.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Status = p.IsActive ? "Active" : "Inactive",
                IsActive = p.IsActive,
                ViewCount = 0, // Ürün görüntüleme tablomuz varsa güncellenir
                SalesCount = p.SalesCount, 
                CreatedAt = p.CreatedAt
            }).ToListAsync();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETPRODUCTS, ex, new { search, status }); throw; }
    }

    public async Task ApproveProductAsync(Guid productId)
    {
        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product != null)
            {
                product.IsActive = true;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_APPROVEPRODUCT, ex, productId); throw; }
    }

    public async Task RejectProductAsync(Guid productId)
    {
        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product != null)
            {
                product.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_REJECTPRODUCT, ex, productId); throw; }
    }

    public async Task SuspendProductAsync(Guid productId)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null) { product.IsActive = false; await _context.SaveChangesAsync(); }
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_SUSPENDPRODUCT, ex, productId); throw; }
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null) { _context.Products.Remove(product); await _context.SaveChangesAsync(); }
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_DELETEPRODUCT, ex, productId); throw; }
    }

    public async Task<List<AdminOrderDto>> GetAllOrdersAsync(string? search = null, string? status = null)
    {
        try
        {
            var query = _context.Orders.Include(o => o.User).AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.OrderNumber.Contains(search) || o.User.FullName.Contains(search));
                
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<OrderStatus>(status, out var parsedStatus))
                    query = query.Where(o => o.Status == parsedStatus);
            }
            
            return await query.OrderByDescending(o => o.CreatedAt).Select(o => new AdminOrderDto
            {
                Id = o.Id, 
                OrderNumber = o.OrderNumber,
                CustomerName = o.User.FullName,
                TotalPrice = o.TotalPrice,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            }).ToListAsync();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETORDERS, ex, new { search, status }); throw; }
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, string newStatus)
    {
        try
        {
            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null && Enum.TryParse<OrderStatus>(newStatus, out var parsedStatus))
            {
                order.Status = parsedStatus;
                await _context.SaveChangesAsync();

                _ = _publishEndpoint.Publish(new OrderStatusChangedEvent
                {
                    OrderId = order.Id,
                    NewStatus = parsedStatus
                });
            }
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_UPDATEORDER, ex, new { orderId, newStatus }); throw; }
    }

    public async Task<AdminReportDto> GetReportAsync(DateTime? from, DateTime? to)
    {
        try
        {
            var startDate = from ?? DateTime.UtcNow.AddMonths(-6);
            var endDate = to ?? DateTime.UtcNow;

            var query = _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);

            var totalOrders = await query.CountAsync();
            var totalRevenue = await query.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped).SumAsync(o => o.TotalPrice);
            var totalCustomers = await query.Select(o => o.UserId).Distinct().CountAsync();
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var ordersByStatus = await query
                .GroupBy(o => o.Status)
                .Select(g => new OrdersByStatusDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            var monthlySales = await query
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new MonthlySalesDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    OrderCount = g.Count(),
                    Revenue = g.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped).Sum(o => o.TotalPrice)
                })
                .OrderBy(m => m.Month)
                .ToListAsync();

            // En çok satan ürünler (Top 10) - Şu anki DB yapısında OrderItem yok, bu yüzden basitçe boş bırakıyoruz veya mock dönüyoruz
            var topProducts = new List<TopProductDto>(); 

            return new AdminReportDto
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalCustomers = totalCustomers,
                AverageOrderValue = averageOrderValue,
                OrdersByStatus = ordersByStatus,
                MonthlySales = monthlySales,
                TopProducts = topProducts
            };
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync("ADM-012", ex, new { from, to });
            throw;
        }
    }
}
