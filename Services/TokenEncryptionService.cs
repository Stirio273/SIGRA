using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace SIGRA.Services;

public sealed class TokenEncryptionService : ITokenEncryptionService
{
    private readonly IDataProtector _protector;

    public TokenEncryptionService(IDataProtectionProvider provider)
    {
        // "purpose" isole ce protector des autres usages
        _protector = provider.CreateProtector("GoogleOAuth2.Tokens.v1");
    }

    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cipherText);

        try
        {
            return _protector.Unprotect(cipherText);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException(
                "Failed to decrypt token. The token may be corrupted or the key has changed.",
                ex);
        }
    }
}
