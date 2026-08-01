using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Nexus.Wishlist.Application;

namespace Nexus.Wishlist.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddWishlistInfrastructure(this IServiceCollection services, string mongoConnectionString, string databaseName)
    {
        var mongoClient = new MongoClient(mongoConnectionString);
        var database = mongoClient.GetDatabase(databaseName);
        services.AddSingleton(database);
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<WishlistService>();
        return services;
    }
}
