using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface IAdminService
{
    // Dashboard
    Task<AdminDashboardStatsDto> GetDashboardStatsAsync();

    // Kullanıcı Yönetimi
    Task<List<AdminUserDto>> GetAllUsersAsync(string? search = null, string? role = null, string? status = null);
    Task SuspendUserAsync(Guid userId);
    Task ActivateUserAsync(Guid userId);
    Task BanUserAsync(Guid userId, bool isPermanent, int days, string reason);
    Task UnbanUserAsync(Guid userId);

    // Ürün Yönetimi
    Task<List<AdminProductDto>> GetAllProductsAsync(string? search = null, string? status = null);
    Task ApproveProductAsync(Guid productId);
    Task RejectProductAsync(Guid productId);
    Task SuspendProductAsync(Guid productId);
    Task DeleteProductAsync(Guid productId);

    // Sipariş Yönetimi
    Task<List<AdminOrderDto>> GetAllOrdersAsync(string? search = null, string? status = null);
    Task UpdateOrderStatusAsync(Guid orderId, string newStatus);

    // Raporlar
    Task<AdminReportDto> GetReportAsync(DateTime? from, DateTime? to);
}
