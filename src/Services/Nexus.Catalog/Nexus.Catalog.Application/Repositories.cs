using Nexus.Catalog.Domain;

namespace Nexus.Catalog.Application;

public interface ICatalogRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Product>> GetByCategoryAsync(string categoryId, int page, int pageSize, CancellationToken ct = default);
    Task<List<Product>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken ct = default);
    Task CreateAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task<long> GetCountAsync(CancellationToken ct = default);
}

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(string id, CancellationToken ct = default);
    Task CreateAsync(Category category, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
}