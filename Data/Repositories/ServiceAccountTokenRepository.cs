using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Data.Repositories;

public sealed class ServiceAccountTokenRepository : IServiceAccountTokenRepository
{
    private readonly AppDbContext _context;
    private readonly ITokenEncryptionService _encryption;
    private readonly ILogger<ServiceAccountTokenRepository> _logger;

    public ServiceAccountTokenRepository(
        AppDbContext context,
        ITokenEncryptionService encryption,
        ILogger<ServiceAccountTokenRepository> logger)
    {
        _context = context;
        _encryption = encryption;
        _logger = logger;
    }

    public async Task<ServiceAccountToken?> GetAsync(
        string email,
        OAuthProvider provider,
        CancellationToken ct = default)
    {
        return await _context.ServiceAccountTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Email == email &&
                x.Provider == provider, ct);
    }

    public async Task SaveAsync(
        string email,
        OAuthProvider provider,
        string accessToken,
        string? refreshToken,
        DateTime? expiresAt,
        string? scopes = null,
        CancellationToken ct = default)
    {
        var encryptedAccess = _encryption.Encrypt(accessToken);
        var encryptedRefresh = refreshToken is not null
            ? _encryption.Encrypt(refreshToken)
            : null;

        var existing = await _context.ServiceAccountTokens
            .FirstOrDefaultAsync(x =>
                x.Email == email &&
                x.Provider == provider, ct);

        if (existing is null)
        {
            _logger.LogInformation(
                "Saving new {Provider} token for {Email}",
                provider, email);

            var token = ServiceAccountToken.Create(
                email, provider,
                encryptedAccess, encryptedRefresh,
                expiresAt, scopes);

            await _context.ServiceAccountTokens.AddAsync(token, ct);
        }
        else
        {
            _logger.LogInformation(
                "Updating {Provider} token for {Email}",
                provider, email);

            existing.UpdateTokens(encryptedAccess, encryptedRefresh, expiresAt);
            _context.ServiceAccountTokens.Update(existing);
        }

        await _context.SaveChangesAsync(ct);
    }
}
