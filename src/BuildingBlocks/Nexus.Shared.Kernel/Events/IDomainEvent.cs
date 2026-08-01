using MediatR;

namespace Nexus.Shared.Kernel.Events;

/// <summary>
/// Interface marcadora para eventos de domínio (DDD).
/// Extende INotification do MediatR para que os eventos possam ser
/// publicados via mediator e tratados por handlers específicos.
/// Um evento de domínio representa algo relevante que aconteceu no
/// domínio (ex: "pedido submetido", "pagamento aprovado").
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>Momento exato em que o evento ocorreu (UTC).</summary>
    DateTime OccurredAt { get; }
}
