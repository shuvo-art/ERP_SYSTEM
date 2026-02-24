namespace Auth.Core.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsRevoked { get; set; }

    // ─── Refresh Token Rotation Fields ───────────────────────────────────────
    // When a token is rotated, the old token points to the new one.
    // This creates a chain: TokenA -> TokenB -> TokenC (current)
    public string? ReplacedByToken { get; set; }

    // TokenFamily groups all tokens in a rotation chain under one identifier.
    // On breach detection (reuse of an old token), we revoke the entire family.
    public string TokenFamily { get; set; } = string.Empty;

    // Navigation property
    public User? User { get; set; }
}
