namespace Nexus.Shared.Messaging.Bus;

/// <summary>
/// Classe base para eventos de integração (comunicação entre serviços).
/// Diferente de Domain Events (intra-serviço), Integration Events são
/// publicados no Message Bus (RabbitMQ) para que outros microsserviços
/// reajam. Ex: quando um pedido é pago, um PaymentApprovedEvent é
/// publicado para que o serviço de Pedidos atualize o status.
/// </summary>
public abstract class IntegrationEvent
{
    /// <summary>ID único do evento (para deduplicação).</summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>Timestamp UTC de quando o evento ocorreu.</summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>Nome do tipo do evento (usado como routing key no RabbitMQ).</summary>
    public string EventType => GetType().Name;
}
