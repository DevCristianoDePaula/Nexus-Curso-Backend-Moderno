using Nexus.Payments.Domain;

namespace Nexus.Payments.Infrastructure;

///
/// <summary>
/// Implementação de gateway de pagamento para ambiente de testes (Sandbox).
/// 
/// Padrão **Adapter / Strategy**: IPaymentGateway define o contrato;
/// SandboxPaymentGateway é uma implementação concreta que simula aprovação/rejeição
/// sem chamar nenhum provedor real (Stripe, PagSeguro, etc.).
/// 
/// Regras de simulação:
/// - Valores acima de R$ 20.000 → rejeitados
/// - Acima de R$ 5.000 com cartão de crédito → rejeitados
/// - Demais casos → aprovados com TransactionId no formato "TXN-YYYYMMDD-NNNNNN"
/// - Reembolso é sempre aprovado
/// </summary>
public class SandboxPaymentGateway : IPaymentGateway
{
    private static int _counter;

    ///
    /// <summary>
    /// Simula o processamento de um pagamento com regras de negócio mockadas.
    /// </summary>
    public Task<PaymentResult> ProcessAsync(Guid orderId, decimal amount, string currency, PaymentMethod method, CancellationToken ct = default)
    {
        // Interlocked.Increment: operação thread-safe para gerar contadores sequenciais.
        Interlocked.Increment(ref _counter);

        // Regra 1: valor acima de R$ 20.000 → rejeitado.
        if (amount > 20000)
        {
            return Task.FromResult(new PaymentResult
            {
                Approved = false,
                FailureReason = "Amount exceeds maximum limit of R$ 20,000"
            });
        }

        // Regra 2: cartão de crédito acima de R$ 5.000 → rejeitado.
        if (amount > 5000 && method == PaymentMethod.CreditCard)
        {
            return Task.FromResult(new PaymentResult
            {
                Approved = false,
                FailureReason = "Amount over R$ 5,000 requires manual review for credit card"
            });
        }

        // Aprovado: gera transaction ID único baseado em data + contador.
        return Task.FromResult(new PaymentResult
        {
            Approved = true,
            TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{_counter:D6}"
        });
    }

    ///
    /// <summary>
    /// Simula o reembolso de uma transação. Sempre aprovado no sandbox.
    /// </summary>
    public Task<PaymentResult> RefundAsync(string transactionId, CancellationToken ct = default)
    {
        return Task.FromResult(new PaymentResult
        {
            Approved = true,
            TransactionId = $"RFN-{transactionId}"
        });
    }
}
