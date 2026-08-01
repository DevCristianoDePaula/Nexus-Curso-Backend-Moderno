using Nexus.Shared.Kernel.Events;

namespace Nexus.Shared.Kernel.Entities;

/// <summary>
/// Classe base para Aggregate Roots (DDD).
/// Um Aggregate Root é a entidade raiz que garante a consistência de um grupo de objetos.
/// Por exemplo: Order é Aggregate Root que contém OrderItems — toda mudança nos items
/// passa pelo Order. Esta classe adiciona suporte a Domain Events, permitindo que
/// o agregado notifique o resto do sistema sobre mudanças importantes.
/// </summary>
public abstract class AggregateRoot : Entity
{
    // Lista interna de eventos de domínio pendentes de publicação
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Eventos de domínio pendentes (público apenas leitura).</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adiciona um evento de domínio à lista.
    /// Deve ser chamado pelas entidades quando algo relevante acontece
    /// (ex: pedido submetido, pagamento aprovado).
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Limpa a lista de eventos (chamado após publicação pelo DomainEventDispatcher).</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
