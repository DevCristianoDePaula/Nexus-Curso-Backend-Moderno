using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus.Shared.Messaging.Bus;

namespace Nexus.Shared.Messaging.Outbox;

public class OutboxProcessor : BackgroundService
{
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(10)];
    private const int MaxRetryCount = 4;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                var pending = await outboxRepo.GetPendingAsync(ct: stoppingToken);

                foreach (var message in pending)
                {
                    try
                    {
                        if (message.RetryCount >= MaxRetryCount)
                        {
                            _logger.LogWarning("Message {Id} exceeded max retries ({RetryCount}), moving to Dead Letter", message.Id, MaxRetryCount);
                            message.MarkFailed($"Max retries exceeded ({MaxRetryCount})");
                            await outboxRepo.UpdateAsync(message, stoppingToken);
                            continue;
                        }

                        var eventType = Type.GetType(message.EventType);
                        if (eventType is null)
                        {
                            message.MarkFailed($"Type {message.EventType} not found");
                            await outboxRepo.UpdateAsync(message, stoppingToken);
                            continue;
                        }

                        var deserialized = System.Text.Json.JsonSerializer.Deserialize(message.Payload, eventType);
                        if (deserialized is null)
                        {
                            message.MarkFailed("Deserialization returned null");
                            await outboxRepo.UpdateAsync(message, stoppingToken);
                            continue;
                        }

                        var publishMethod = typeof(IMessageBus)
                            .GetMethod(nameof(IMessageBus.PublishAsync))?
                            .MakeGenericMethod(eventType);

                        if (publishMethod is not null)
                        {
                            await (Task)publishMethod.Invoke(bus, [deserialized, stoppingToken])!;
                        }

                        message.MarkProcessed();
                        await outboxRepo.UpdateAsync(message, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Outbox processing failed for message {Id} (attempt {Retry})", message.Id, message.RetryCount + 1);
                        message.MarkFailed(ex.Message);
                        await outboxRepo.UpdateAsync(message, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processor encountered an error");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
