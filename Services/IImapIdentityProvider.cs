namespace SIGRA.Services;

public interface IImapIdentityProvider
{
    /// <summary>
    /// Returns the primary mail identity used by MailKit during authentication.
    /// </summary>
    string GetMailboxIdentity();

    /// <summary>
    /// Returns the current authorization material for the identity.
    /// With OAuth2, this should return a valid access token.
    /// With standard auth, this may return the password string.
    /// </summary>
    Task<string> GetAuthorizationMaterialAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Optional external authorization flow.
    /// Returns the redirect URL to initiate login, or null when not supported.
    /// </summary>
    Task<string?> GetAuthorizationUrlAsync(string? redirectUri = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges an authorization callback code for a fresh token material.
    /// Returns true when the exchange has been persisted for later use.
    /// </summary>
    Task<bool> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default);
}
