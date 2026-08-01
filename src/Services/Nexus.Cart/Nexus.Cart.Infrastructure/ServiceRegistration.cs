using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Cart.Application;

namespace Nexus.Cart.Infrastructure;

///
/// <summary>
/// Classe estática de registro de dependências do módulo Cart.Infrastructure.
/// 
/// Padrão **ServiceRegistration**: configura o Redis como cache distribuído,
/// registra o repositório RedisCartRepository e o serviço de aplicação CartService.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddCartInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis")
            ?? "localhost:6379,password=Nexus@2026#";

        // Configura o Redis como mecanismo de cache distribuído (IDistributedCache).
        services.AddStackExchangeRedisCache(options =>
            options.Configuration = redisConnection);

        services.AddScoped<ICartRepository, RedisCartRepository>();
        services.AddScoped<CartService>();

        return services;
    }
}
