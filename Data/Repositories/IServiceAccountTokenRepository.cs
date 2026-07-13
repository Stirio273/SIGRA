using SIGRA.Data.Enums;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IServiceAccountTokenRepository
{
    Task<ServiceAccountToken?> GetAsync(
        string email,
        OAuthProvider provider,
        CancellationToken ct = default);

    Task SaveAsync(
        string email,
        OAuthProvider provider,
        string accessToken,
        string? refreshToken,
        DateTime? expiresAt,
        string? scopes = null,
        CancellationToken ct = default);
}
