using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Nexus.Cart.Application;

namespace Nexus.Cart.Infrastructure;

///
/// <summary>
/// Repositório de Carrinho implementado com Redis (IDistributedCache).
/// 
/// Padrão **Repository**: abstrai o armazenamento volátil do carrinho de compras.
/// Diferente dos demais repositórios (MongoDB/EF Core), o carrinho usa cache distribuído
/// com expiração deslizante — ideal para dados temporários que não exigem persistência relacional.
/// 
/// Características:
/// - Chave no formato "cart:{userId}"
/// - Expiração deslizante de 24 horas (SlidingExpiration)
/// - Serialização JSON via Newtonsoft.Json
/// </summary>
public class RedisCartRepository : ICartRepository
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan CartTtl = TimeSpan.FromHours(24);

    public RedisCartRepository(IDistributedCache cache) => _cache = cache;

    ///
    /// <summary>
    /// Obtém o carrinho pelo ID do usuário. Retorna null se não existir ou expirou.
    /// </summary>
    public async Task<Domain.Cart?> GetAsync(string userId, CancellationToken ct = default)
    {
        var key = $"cart:{userId}";
        var data = await _cache.GetStringAsync(key, ct);
        return data is null ? null : JsonConvert.DeserializeObject<Domain.Cart>(data);
    }

    ///
    /// <summary>
    /// Salva o carrinho no Redis com expiração deslizante (renova a cada acesso).
    /// </summary>
    public async Task SaveAsync(Domain.Cart cart, CancellationToken ct = default)
    {
        var key = $"cart:{cart.UserId}";
        var data = JsonConvert.SerializeObject(cart);
        await _cache.SetStringAsync(key, data, new DistributedCacheEntryOptions
        {
            SlidingExpiration = CartTtl
        }, ct);
    }

    ///
    /// <summary>
    /// Remove o carrinho do Redis.
    /// </summary>
    public async Task DeleteAsync(string userId, CancellationToken ct = default)
    {
        var key = $"cart:{userId}";
        await _cache.RemoveAsync(key, ct);
    }
}
