namespace Nexus.Users.Domain;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; }
    public string UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private RefreshToken() { }

    public RefreshToken(string userId, TimeSpan ttl)
    {
        Token = Guid.NewGuid().ToString("N");
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.Add(ttl);
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}
