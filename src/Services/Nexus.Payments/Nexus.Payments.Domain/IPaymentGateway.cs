namespace Nexus.Payments.Domain;

/// <summary>
/// Interface de porta de saída (driven port) na terminologia da Clean Architecture.
/// Define o contrato que qualquer gateway de pagamento externo deve implementar.
/// A camada de infraestrutura será responsável por implementar esta interface
/// (ex: integração com Stripe, PagSeguro, Mercado Pago, etc.).
/// Isso mantém o domínio desacoplado de provedores externos.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Processa um pagamento junto ao gateway externo.
    /// Retorna um PaymentResult indicando sucesso ou falha.
    /// </summary>
    Task<PaymentResult> ProcessAsync(Guid orderId, decimal amount, string currency, PaymentMethod method, CancellationToken ct = default);

    /// <summary>
    /// Solicita o reembolso de uma transação já aprovada.
    /// </summary>
    Task<PaymentResult> RefundAsync(string transactionId, CancellationToken ct = default);
}

/// <summary>
/// Objeto de retorno do gateway de pagamento.
/// Contém o resultado da operação (aprovado/recusado) e dados da transação.
/// </summary>
public class PaymentResult
{
    // Indica se a transação foi aprovada
    public bool Approved { get; init; }

    // ID da transação no gateway (presente se aprovada)
    public string? TransactionId { get; init; }

    // Motivo da falha (presente se recusada)
    public string? FailureReason { get; init; }
}
