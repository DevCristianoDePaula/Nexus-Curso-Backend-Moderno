using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Orders.Application;

namespace Nexus.Orders.Infrastructure;

///
/// <summary>
/// Classe estática de registro de dependências do módulo Orders.Infrastructure.
/// 
/// Padrão **ServiceRegistration**: centraliza a configuração do EF Core,
/// repositórios, serviços de aplicação e MediatR.
/// 
/// Registros:
/// - NexusOrderDbContext com SQL Server
/// - IOrderRepository (repositório)
/// - OrderService e DomainEventDispatcher (aplicação)
/// - MediatR (registro automático de handlers de eventos e commands)
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddOrdersInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrdersDb")
            ?? "Server=localhost;Database=Nexus_Orders;User Id=sa;Password=Nexus@2026#;TrustServerCertificate=True";

        // Configura o DbContext com SQL Server.
        services.AddDbContext<NexusOrderDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<OrderService>();
        services.AddScoped<DomainEventDispatcher>();

        // MediatR: registra handlers (eventos, commands, queries) do assembly onde OrderService reside.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<OrderService>());

        return services;
    }
}
