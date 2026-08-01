using MediatR;
using Nexus.Shared.Kernel.Entities;
using Nexus.Shared.Kernel.Events;

namespace Nexus.Orders.Application;

///
/// <summary>
/// Dispatcher de Eventos de Domínio.
/// Implementa o padrão **Domain Events** do DDD: após uma operação no Aggregate Root,
/// os eventos registrados são publicados via MediatR para que handlers assíncronos
/// (notificações, integrações, sagas) possam reagir.
/// 
/// Isso mantém o domínio puro (sem dependências de infraestrutura) e permite
/// efeitos colaterais desacoplados.
/// </summary>
public class DomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator) => _mediator = mediator;

    ///
    /// <summary>
    /// Publica todos os eventos de domínio pendentes no aggregate e os limpa.
    /// </summary>
    public async Task DispatchAsync(AggregateRoot aggregate, CancellationToken ct = default)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            await _mediator.Publish(domainEvent, ct);
        }
        aggregate.ClearDomainEvents();
    }
}
