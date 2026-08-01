using Microsoft.EntityFrameworkCore;

namespace Nexus.Shared.Messaging.Outbox;

/// <summary>
/// Implementação do repositório Outbox usando EF Core + SQL Server.
/// O OutboxDbContext gerencia a tabela OutboxMessages que armazena
/// eventos a serem publicados no RabbitMQ.
/// </summary>
public class EfOutboxRepository : IOutboxRepository
{
    private readonly OutboxDbContext _context;

    public EfOutboxRepository(OutboxDbContext context) => _context = context;

    /// <summary>
    /// Adiciona mensagem à tabela Outbox (na mesma transação do agregado
    /// quando usado com TransactionScope ou contexto compartilhado).
    /// </summary>
    public async Task AddAsync(OutboxMessage message, CancellationToken ct = default)
    {
        await _context.OutboxMessages.AddAsync(message, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Busca as N mensagens mais antigas com status "Pending".
    /// O OutboxProcessor chama este método periodicamente.
    /// </summary>
    public async Task<List<OutboxMessage>> GetPendingAsync(int batchSize = 50, CancellationToken ct = default)
    {
        return await _context.OutboxMessages
            .Where(m => m.Status == "Pending")
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    /// <summary>Atualiza o status da mensagem (Processed ou Failed).</summary>
    public async Task UpdateAsync(OutboxMessage message, CancellationToken ct = default)
    {
        _context.OutboxMessages.Update(message);
        await _context.SaveChangesAsync(ct);
    }
}

/// <summary>
/// DbContext específico para a tabela de Outbox.
/// Deve ser registrado no DI do serviço que usa mensageria.
/// </summary>
public class OutboxDbContext : DbContext
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configura a tabela OutboxMessages com índices e tamanhos de coluna
        builder.Entity<OutboxMessage>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.EventType).HasMaxLength(500);
            e.Property(m => m.Status).HasMaxLength(20);
            e.HasIndex(m => m.Status); // Índice para consulta de mensagens pendentes
        });
    }
}
