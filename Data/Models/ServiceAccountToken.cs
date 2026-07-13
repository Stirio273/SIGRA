namespace SIGRA.Data.Models;

public class ServiceAccountToken
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string EncryptedAccessToken { get; set; } = null!;
    public string? EncryptedRefreshToken { get; set; }
    public string TokenType { get; set; } = null!;
    public string? Scopes { get; set; }
    public DateTime? AccessTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
