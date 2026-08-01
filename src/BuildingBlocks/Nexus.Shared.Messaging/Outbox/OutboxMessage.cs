namespace Nexus.Shared.Messaging.Outbox;

/// <summary>
/// Representa uma mensagem na tabela Outbox (Outbox Pattern).
/// Cada mensagem contém o evento serializado (JSON) e seu status.
/// O OutboxProcessor (BackgroundService) varre as mensagens "Pending"
/// e as publica no RabbitMQ, atualizando o status para "Processed".
/// </summary>
public class OutboxMessage
{
    /// <summary>ID único da mensagem.</summary>
    public Guid Id { get; private set; }

    /// <summary>Nome do tipo do evento (ex: "OrderSubmittedEvent").</summary>
    public string EventType { get; private set; }

    /// <summary>Payload serializado em JSON.</summary>
    public string Payload { get; private set; }

    /// <summary>Status: Pending | Processed | Failed.</summary>
    public string Status { get; private set; }

    /// <summary>Quantas vezes tentou publicar (incrementado em falha).</summary>
    public int RetryCount { get; private set; }

    /// <summary>Timestamp de criação.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Timestamp de processamento (preenchido em sucesso).</summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>Mensagem de erro da última tentativa.</summary>
    public string? Error { get; private set; }

    // Construtor sem parâmetros para EF Core
    private OutboxMessage() { }

    /// <summary>Cria uma nova mensagem com status "Pending".</summary>
    public OutboxMessage(string eventType, string payload)
    {
        Id = Guid.NewGuid();
        EventType = eventType;
        Payload = payload;
        Status = "Pending";
        RetryCount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Marca como processada com sucesso.</summary>
    public void MarkProcessed()
    {
        Status = "Processed";
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>Marca como falha, incrementando tentativa.</summary>
    public void MarkFailed(string error)
    {
        Status = "Failed";
        Error = error;
        RetryCount++;
    }
}
