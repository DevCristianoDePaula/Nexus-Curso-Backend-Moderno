using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Nexus.Reviews.Application;

namespace Nexus.Reviews.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddReviewsInfrastructure(this IServiceCollection services, string mongoConnectionString, string databaseName)
    {
        var mongoClient = new MongoClient(mongoConnectionString);
        var database = mongoClient.GetDatabase(databaseName);
        services.AddSingleton(database);
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ReviewService>();
        return services;
    }
}
