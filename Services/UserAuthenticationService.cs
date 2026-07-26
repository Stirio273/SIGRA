using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface IUserAuthenticationService
{
    Task<Utilisateur?> GetAuthorizedUserAsync(string username);
    Task<bool> IsUserAuthorizedAsync(string username);
}

public class UserAuthenticationService : IUserAuthenticationService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserAuthenticationService> _logger;

    public UserAuthenticationService(AppDbContext dbContext, ILogger<UserAuthenticationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Utilisateur?> GetAuthorizedUserAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var normalized = username.Trim().ToLower();

        return await _dbContext.Utilisateurs
            .Where(u => u.Actif && (
                u.Email.ToLower() == normalized ||
                u.IdentifiantAd.ToLower() == normalized))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsUserAuthorizedAsync(string username)
    {
        var user = await GetAuthorizedUserAsync(username);
        return user != null;
    }
}
