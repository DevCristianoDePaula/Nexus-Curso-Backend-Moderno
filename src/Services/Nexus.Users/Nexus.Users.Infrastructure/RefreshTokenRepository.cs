using Microsoft.EntityFrameworkCore;
using Nexus.Users.Domain;

namespace Nexus.Users.Infrastructure;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly NexusIdentityDbContext _context;

    public RefreshTokenRepository(NexusIdentityDbContext context) => _context = context;

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await _context.Set<RefreshToken>().AddAsync(token, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.Token == token, ct);
    }

    public async Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        var activeTokens = await _context.Set<RefreshToken>()
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
            token.Revoke();

        await _context.SaveChangesAsync(ct);
    }
}
