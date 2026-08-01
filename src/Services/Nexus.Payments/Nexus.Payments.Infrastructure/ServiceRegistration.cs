using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Nexus.Payments.Application;
using Nexus.Payments.Domain;

namespace Nexus.Payments.Infrastructure;

///
/// <summary>
/// Classe estática de registro de dependências do módulo Payments.Infrastructure.
/// 
/// Padrão **ServiceRegistration**: configura MongoDB, repositórios e gateway de pagamento.
/// 
/// Particularidades:
/// - IPaymentGateway é registrado como Singleton (SandboxGateway não tem estado mutável relevante)
/// - Demais serviços são Scoped (um por request HTTP)
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb")
            ?? "mongodb://nexus:Nexus%402026%23@localhost:27017";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("nexus_payments");
        });

        services.AddScoped<IPaymentRepository, MongoPaymentRepository>();
        services.AddSingleton<IPaymentGateway, SandboxPaymentGateway>();
        services.AddScoped<PaymentService>();

        return services;
    }
}
