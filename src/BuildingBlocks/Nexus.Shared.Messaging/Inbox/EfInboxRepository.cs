using Microsoft.EntityFrameworkCore;

namespace Nexus.Shared.Messaging.Inbox;

public class EfInboxRepository : IInboxRepository
{
    private readonly InboxDbContext _context;

    public EfInboxRepository(InboxDbContext context) => _context = context;

    public async Task<bool> ExistsAsync(string messageId, CancellationToken ct = default)
    {
        return await _context.InboxMessages.AnyAsync(m => m.MessageId == messageId, ct);
    }

    public async Task AddAsync(InboxMessage message, CancellationToken ct = default)
    {
        await _context.InboxMessages.AddAsync(message, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(InboxMessage message, CancellationToken ct = default)
    {
        _context.InboxMessages.Update(message);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<InboxMessage>> GetPendingAsync(int batchSize = 50, CancellationToken ct = default)
    {
        return await _context.InboxMessages
            .Where(m => m.Status == "Pending")
            .OrderBy(m => m.ReceivedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }
}

public class InboxDbContext : DbContext
{
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public InboxDbContext(DbContextOptions<InboxDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<InboxMessage>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.MessageId).IsUnique();
            e.Property(m => m.EventType).HasMaxLength(500);
            e.Property(m => m.Status).HasMaxLength(20);
            e.HasIndex(m => m.Status);
        });
    }
}
