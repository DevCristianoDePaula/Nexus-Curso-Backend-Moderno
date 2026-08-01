using Nexus.Shared.Kernel.Events;

namespace Nexus.Payments.Domain;

/// <summary>
/// Evento de domínio disparado quando um pagamento é aprovado.
/// O serviço de Pedidos deve escutar este evento para confirmar o pedido.
/// </summary>
public class PaymentApprovedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public string TransactionId { get; }

    public PaymentApprovedEvent(Guid paymentId, Guid orderId, string transactionId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        TransactionId = transactionId;
    }
}

/// <summary>
/// Evento de domínio disparado quando um pagamento é recusado.
/// O serviço de Pedidos deve escutar este evento para notificar o cliente
/// e permitir que tente outro meio de pagamento.
/// </summary>
public class PaymentDeclinedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public string Reason { get; }

    public PaymentDeclinedEvent(Guid paymentId, Guid orderId, string reason)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Reason = reason;
    }
}
