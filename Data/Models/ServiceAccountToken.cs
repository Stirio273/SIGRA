using System;
using System.Collections.Generic;
using SIGRA.Data.Enums;

namespace SIGRA.Data.Models;

public partial class ServiceAccountToken
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;
    
    public string EncryptedAccessToken { get; set; } = null!;

    public string? EncryptedRefreshToken { get; set; }

    public string? Scopes { get; set; }

    public DateTime? AccessTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
