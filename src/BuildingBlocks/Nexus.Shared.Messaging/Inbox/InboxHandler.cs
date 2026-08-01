using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.Shared.Messaging.Bus;

namespace Nexus.Shared.Messaging.Inbox;

public static class InboxHandler
{
    public static async Task ProcessAsync<T>(
        T message,
        string messageId,
        Func<T, Task> handler,
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        CancellationToken ct = default) where T : class
    {
        using var scope = scopeFactory.CreateScope();
        var inboxRepo = scope.ServiceProvider.GetRequiredService<IInboxRepository>();

        if (await inboxRepo.ExistsAsync(messageId, ct))
        {
            logger.LogInformation("Message {MessageId} already processed, skipping", messageId);
            return;
        }

        var eventType = typeof(T).Name;
        var payload = System.Text.Json.JsonSerializer.Serialize(message);
        var inboxMessage = new InboxMessage(messageId, eventType, payload);

        await inboxRepo.AddAsync(inboxMessage, ct);

        try
        {
            await handler(message);
            inboxMessage.MarkProcessed();
            await inboxRepo.UpdateAsync(inboxMessage, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process message {MessageId}", messageId);
            inboxMessage.MarkFailed(ex.Message);
            await inboxRepo.UpdateAsync(inboxMessage, ct);
            throw;
        }
    }
}
