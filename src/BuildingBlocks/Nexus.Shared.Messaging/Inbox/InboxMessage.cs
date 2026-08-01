namespace Nexus.Shared.Messaging.Inbox;

public class InboxMessage
{
    public Guid Id { get; private set; }
    public string MessageId { get; private set; }
    public string EventType { get; private set; }
    public string Payload { get; private set; }
    public string Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }

    private InboxMessage() { }

    public InboxMessage(string messageId, string eventType, string payload)
    {
        Id = Guid.NewGuid();
        MessageId = messageId;
        EventType = eventType;
        Payload = payload;
        Status = "Pending";
        RetryCount = 0;
        ReceivedAt = DateTime.UtcNow;
    }

    public void MarkProcessed()
    {
        Status = "Processed";
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = "Failed";
        Error = error;
        RetryCount++;
    }
}
