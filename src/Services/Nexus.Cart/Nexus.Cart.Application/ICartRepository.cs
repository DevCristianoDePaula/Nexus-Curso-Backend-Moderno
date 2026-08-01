namespace Nexus.Cart.Application;

public interface ICartRepository
{
    Task<Domain.Cart?> GetAsync(string userId, CancellationToken ct = default);
    Task SaveAsync(Domain.Cart cart, CancellationToken ct = default);
    Task DeleteAsync(string userId, CancellationToken ct = default);
}
