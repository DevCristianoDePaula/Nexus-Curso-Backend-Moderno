using Nexus.Payments.Domain;

namespace Nexus.Payments.Application;

///
/// <summary>
/// Serviço de aplicação de Pagamentos.
/// Coordena o processamento, consulta e reembolso de pagamentos.
/// 
/// Padrões aplicados:
/// - **Application Service**: orquestra o domínio (Payment) e interfaces externas.
/// - **Repository Pattern**: IPaymentRepository abstrai o MongoDB.
/// - **Strategy / Adapter Pattern**: IPaymentGateway abstrai provedores de pagamento
///   (SandboxPaymentGateway é uma implementação concreta de teste).
/// </summary>
public class PaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentGateway _gateway;

    public PaymentService(IPaymentRepository repository, IPaymentGateway gateway)
    {
        _repository = repository;
        _gateway = gateway;
    }

    ///
    /// <summary>
    /// Processa um pagamento: cria a entidade Payment, envia ao gateway,
    /// aprova ou declina com base na resposta do gateway e persiste o resultado.
    /// </summary>
    public async Task<Payment> ProcessPaymentAsync(Guid orderId, ProcessPaymentRequest request, CancellationToken ct = default)
    {
        var payment = new Payment(orderId, request.Amount, request.Currency, request.Method);
        await _repository.CreateAsync(payment, ct);

        var result = await _gateway.ProcessAsync(orderId, request.Amount, request.Currency, request.Method, ct);

        if (result.Approved && result.TransactionId is not null)
        {
            payment.Approve(result.TransactionId);
        }
        else
        {
            payment.Decline(result.FailureReason ?? "Payment declined by gateway");
        }

        await _repository.UpdateAsync(payment, ct);
        return payment;
    }

    ///
    /// <summary>
    /// Obtém um pagamento pelo ID.
    /// </summary>
    public async Task<Payment?> GetPaymentAsync(Guid id, CancellationToken ct = default)
    {
        return await _repository.GetByIdAsync(id, ct);
    }

    ///
    /// <summary>
    /// Obtém um pagamento associado a um pedido.
    /// </summary>
    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _repository.GetByOrderIdAsync(orderId, ct);
    }

    ///
    /// <summary>
    /// Reembolsa um pagamento: chama o gateway para estornar e atualiza o status da entidade.
    /// </summary>
    public async Task<Payment?> RefundPaymentAsync(Guid id, CancellationToken ct = default)
    {
        var payment = await _repository.GetByIdAsync(id, ct);
        if (payment is null) return null;

        if (payment.TransactionId is not null)
        {
            await _gateway.RefundAsync(payment.TransactionId, ct);
        }

        payment.Refund();
        await _repository.UpdateAsync(payment, ct);
        return payment;
    }
}

///
/// <summary>
/// DTO de entrada para processar um pagamento.
/// </summary>
public class ProcessPaymentRequest
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "BRL";
    public PaymentMethod Method { get; init; }
}
