namespace SIGRA.Data.Models;

using SIGRA.Data.Enums;

public class ServiceAccountToken
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public OAuthProvider Provider { get; set; }
    public string EncryptedAccessToken { get; set; } = null!;
    public string? EncryptedRefreshToken { get; set; }
    public string? Scopes { get; set; }
    public DateTime? AccessTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static ServiceAccountToken Create(
        string email,
        OAuthProvider provider,
        string encryptedAccessToken,
        string? encryptedRefreshToken,
        DateTime? expiresAt,
        string? scopes = null)
    {
        return new ServiceAccountToken
        {
            Email = email,
            Provider = provider,
            EncryptedAccessToken = encryptedAccessToken,
            EncryptedRefreshToken = encryptedRefreshToken,
            Scopes = scopes,
            AccessTokenExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateTokens(string encryptedAccessToken, string? encryptedRefreshToken, DateTime? expiresAt)
    {
        EncryptedAccessToken = encryptedAccessToken;
        EncryptedRefreshToken = encryptedRefreshToken;
        AccessTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }
}
