using Microsoft.Extensions.DependencyInjection;
using Nexus.Shared.Messaging.Bus;
using Nexus.Shared.Messaging.Inbox;
using Nexus.Shared.Messaging.Outbox;

namespace Nexus.Shared.Messaging;

public static class ServiceRegistration
{
    public static IServiceCollection AddNexusMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IMessageBus, RabbitMqBus>();
        services.AddScoped<IOutboxRepository, EfOutboxRepository>();
        services.AddScoped<IInboxRepository, EfInboxRepository>();
        services.AddHostedService<OutboxProcessor>();
        return services;
    }
}
