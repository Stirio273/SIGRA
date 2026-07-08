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

    public async Task<Utilisateur?> GetAuthorizedUserAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }
        // Normalize username for comparison (case-insensitive)
        return await _dbContext.Utilisateurs
            .Where(u => u.Email.ToLower() == email.ToLower() && u.EstActif)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsUserAuthorizedAsync(string email)
    {
        var user = await GetAuthorizedUserAsync(email);
        return user != null;
    }
}
