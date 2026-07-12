using ETicaret.Data.Entities;

namespace ETicaret.Data.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetUserOrdersAsync(Guid userId);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
}
