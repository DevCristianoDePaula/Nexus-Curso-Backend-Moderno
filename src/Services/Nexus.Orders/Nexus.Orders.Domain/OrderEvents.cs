using Nexus.Shared.Kernel.Events;

namespace Nexus.Orders.Domain;

/// <summary>
/// Evento de domínio disparado quando um pedido é submetido.
/// Contém os dados necessários para que outros serviços (Pagamento, Estoque)
/// possam reagir à submissão do pedido.
/// </summary>
public class OrderSubmittedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string CustomerId { get; }
    public decimal TotalAmount { get; }

    public OrderSubmittedEvent(Guid orderId, string customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}

/// <summary>
/// Evento de domínio disparado quando o pagamento do pedido é confirmado.
/// Notifica outros serviços que o pedido pode seguir para separação/envio.
/// </summary>
public class OrderPaidEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string PaymentId { get; }

    public OrderPaidEvent(Guid orderId, string paymentId)
    {
        OrderId = orderId;
        PaymentId = paymentId;
    }
}

/// <summary>
/// Evento de domínio disparado quando um pedido é cancelado.
/// Inclui o motivo do cancelamento para auditoria e notificação.
/// </summary>
public class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string Reason { get; }

    public OrderCancelledEvent(Guid orderId, string reason)
    {
        OrderId = orderId;
        Reason = reason;
    }
}
