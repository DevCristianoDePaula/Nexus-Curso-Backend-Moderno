namespace Nexus.Shared.Kernel.Events;

/// <summary>
/// Classe base abstrata para eventos de domínio (DDD).
/// Fornece o timestamp de ocorrência automaticamente.
/// Subclasses devem representar eventos específicos do negócio,
/// como OrderSubmittedEvent ou PaymentApprovedEvent.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>Timestamp UTC do momento em que o evento foi criado.</summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
