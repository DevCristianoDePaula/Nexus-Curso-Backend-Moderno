namespace Nexus.Shared.Messaging.Outbox;

/// <summary>
/// Interface do repositório Outbox (padrão Outbox Pattern).
/// Garante publicação confiável de eventos: a mensagem é salva na MESMA
/// transação do agregado, e um background service (OutboxProcessor)
/// a publica no RabbitMQ posteriormente. Se falhar, tenta de novo.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>Adiciona mensagem à tabela Outbox.</summary>
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);

    /// <summary>Busca mensagens pendentes de processamento (batch).</summary>
    Task<List<OutboxMessage>> GetPendingAsync(int batchSize = 50, CancellationToken ct = default);

    /// <summary>Atualiza status da mensagem (Processed/Failed).</summary>
    Task UpdateAsync(OutboxMessage message, CancellationToken ct = default);
}
