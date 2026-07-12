namespace ETicaret.Business.DTOs;

public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveUsers { get; set; }
    public int PendingOrders { get; set; }
    public int PendingProducts { get; set; }
    public int TotalViolations { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
