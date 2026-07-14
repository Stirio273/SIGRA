using SIGRA.Data.Enums;

namespace SIGRA.Data.Models;

public partial class ServiceAccountToken
{
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