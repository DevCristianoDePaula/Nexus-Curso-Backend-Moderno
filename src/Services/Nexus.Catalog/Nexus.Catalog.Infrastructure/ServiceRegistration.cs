using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Nexus.Catalog.Application;

namespace Nexus.Catalog.Infrastructure;

///
/// <summary>
/// Classe estática de registro de dependências do módulo Catalog.Infrastructure.
/// 
/// Padrão **ServiceRegistration**: método de extensão em IServiceCollection
/// que centraliza a configuração de DI (Dependency Injection) para o módulo.
/// Segue a convenção de módulos modulares — cada serviço possui seu próprio registro,
/// facilitando a composição na aplicação principal (compor módulos = chamar Add*Infrastructure).
/// 
/// Registros:
/// - IMongoClient: Singleton (reutiliza conexão com MongoDB)
/// - IMongoDatabase: Scoped (cria um escopo por request)
/// - ICatalogRepository e ICategoryRepository: Scoped (conexões por request)
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ConnectionString definida em appsettings.json, com fallback para dev local.
        var connectionString = configuration.GetConnectionString("MongoDb")
            ?? "mongodb://nexus:Nexus@2026#@localhost:27017";

        // Singleton: uma única instância do MongoClient por aplicação (reuso de conexão pool).
        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        // Scoped: um IMongoDatabase por request — garante isolamento transacional.
        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("nexus_catalog");
        });

        // Registro dos repositórios — a aplicação depende das interfaces, não das implementações.
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}