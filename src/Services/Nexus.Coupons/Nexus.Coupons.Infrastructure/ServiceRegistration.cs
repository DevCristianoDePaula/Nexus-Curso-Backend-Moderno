using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Nexus.Coupons.Application;

namespace Nexus.Coupons.Infrastructure;

///
/// <summary>
/// Classe estática de registro de dependências do módulo Coupons.Infrastructure.
/// 
/// Padrão **ServiceRegistration**: configura MongoDB, repositório e serviço de aplicação.
/// 
/// Registros:
/// - IMongoClient: Singleton (pool de conexões)
/// - IMongoDatabase: Scoped
/// - ICouponRepository e CouponService: Scoped
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddCouponsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb")
            ?? "mongodb://nexus:Nexus@2026#@localhost:27017";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("nexus_coupons");
        });

        services.AddScoped<ICouponRepository, MongoCouponRepository>();
        services.AddScoped<CouponService>();

        return services;
    }
}
