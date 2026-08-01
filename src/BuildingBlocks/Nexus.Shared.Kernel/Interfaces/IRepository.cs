using Nexus.Shared.Kernel.Entities;

namespace Nexus.Shared.Kernel.Interfaces;

/// <summary>
/// Interface genérica de repositório (padrão Repository).
/// Define operações CRUD básicas para qualquer Aggregate Root.
/// As implementações concretas ficam na camada de Infrastructure
/// (MongoDB, EF Core, etc.) seguindo o princípio da Inversão de Dependência.
/// </summary>
public interface IRepository<T> where T : AggregateRoot
{
    /// <summary>Busca entidade por ID.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Persiste nova entidade.</summary>
    Task CreateAsync(T entity, CancellationToken ct = default);

    /// <summary>Atualiza entidade existente.</summary>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>Remove entidade.</summary>
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
