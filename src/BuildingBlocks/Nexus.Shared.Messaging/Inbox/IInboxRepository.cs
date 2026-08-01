namespace Nexus.Shared.Messaging.Inbox;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken ct = default);
    Task AddAsync(InboxMessage message, CancellationToken ct = default);
    Task UpdateAsync(InboxMessage message, CancellationToken ct = default);
    Task<List<InboxMessage>> GetPendingAsync(int batchSize = 50, CancellationToken ct = default);
}
