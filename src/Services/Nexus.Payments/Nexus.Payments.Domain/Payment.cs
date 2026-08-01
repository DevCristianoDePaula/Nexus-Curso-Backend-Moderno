using Nexus.Shared.Kernel.Entities;

namespace Nexus.Payments.Domain;

/// <summary>
/// Agregado raiz do domínio de Pagamentos. Representa uma transação financeira
/// associada a um pedido. Gerencia o ciclo de vida do pagamento:
/// Pendente → Aprovado | Recusado → Reembolsado.
/// Cada transição dispara eventos de domínio para que o serviço de Pedidos
/// possa reagir (confirmar ou cancelar o pedido).
/// </summary>
public class Payment : AggregateRoot
{
    // ID do pedido associado a este pagamento
    public Guid OrderId { get; private set; }

    // Valor total do pagamento (deve ser positivo)
    public decimal Amount { get; private set; }

    // Moeda do pagamento (ex: BRL)
    public string Currency { get; private set; }

    // Meio de pagamento escolhido: Cartão de Crédito, Pix ou Boleto
    public PaymentMethod Method { get; private set; }

    // Status atual do pagamento no gateway financeiro
    public PaymentStatus Status { get; private set; }

    // ID da transação retornado pelo gateway (preenchido após aprovação)
    public string? TransactionId { get; private set; }

    // Motivo da recusa (preenchido se o pagamento for recusado)
    public string? FailureReason { get; private set; }

    // Construtor privado exigido pelo Entity Framework
    private Payment() { }

    /// <summary>
    /// Cria um novo pagamento com status Pendente.
    /// </summary>
    public Payment(Guid orderId, decimal amount, string currency, PaymentMethod method)
    {
        OrderId = orderId;
        Amount = amount > 0 ? amount : throw new ArgumentException("Amount must be positive");
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Method = method;
        Status = PaymentStatus.Pending;
    }

    /// <summary>
    /// Aprova o pagamento com o ID da transação do gateway.
    /// Dispara PaymentApprovedEvent para que o pedido seja confirmado.
    /// </summary>
    public void Approve(string transactionId)
    {
        TransactionId = transactionId ?? throw new ArgumentNullException(nameof(transactionId));
        Status = PaymentStatus.Approved;
        Touch();
        AddDomainEvent(new PaymentApprovedEvent(Id, OrderId, transactionId));
    }

    /// <summary>
    /// Recusa o pagamento com o motivo fornecido pelo gateway.
    /// Dispara PaymentDeclinedEvent para que o pedido seja notificado.
    /// </summary>
    public void Decline(string reason)
    {
        FailureReason = reason;
        Status = PaymentStatus.Declined;
        Touch();
        AddDomainEvent(new PaymentDeclinedEvent(Id, OrderId, reason));
    }

    /// <summary>
    /// Reembolsa o pagamento. Só pode ser feito se o pagamento foi aprovado
    /// anteriormente — não é possível reembolsar um pagamento pendente ou recusado.
    /// </summary>
    public void Refund()
    {
        if (Status != PaymentStatus.Approved)
            throw new InvalidOperationException("Only approved payments can be refunded");
        Status = PaymentStatus.Refunded;
        Touch();
    }
}

/// <summary>
/// Meios de pagamento suportados pela plataforma.
/// CreditCard: Cartão de crédito (processamento online)
/// Pix: Transferência instantânea brasileira
/// Boleto: Boleto bancário (pagamento offline)
/// </summary>
public enum PaymentMethod
{
    CreditCard,
    Pix,
    Boleto
}

/// <summary>
/// Estados possíveis de um pagamento.
/// Pending: Aguardando processamento do gateway
/// Approved: Pagamento confirmado
/// Declined: Pagamento recusado
/// Refunded: Estornado/reembolsado
/// </summary>
public enum PaymentStatus
{
    Pending,
    Approved,
    Declined,
    Refunded
}
