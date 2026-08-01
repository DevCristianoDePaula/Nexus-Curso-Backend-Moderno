using Nexus.Orders.Domain;

namespace Nexus.Orders.Application;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Order>> GetByCustomerAsync(string customerId, int page, int pageSize, CancellationToken ct = default);
    Task CreateAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
