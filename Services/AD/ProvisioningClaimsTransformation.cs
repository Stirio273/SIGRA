using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public class ProvisioningClaimsTransformation : IClaimsTransformation
{
    private readonly AppDbContext _db;

    public ProvisioningClaimsTransformation(AppDbContext db) => _db = db;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!principal.IsInRole(@"DEV\SIGRA-Autorises"))
            return principal;

        var samAccountName = principal.Identity!.Name!.Split('\\')[1];
        var user = await _db.Utilisateurs
            .Include(u => u.IdRoleNavigation)
            .FirstOrDefaultAsync(u => u.IdentifiantAd == samAccountName);

        if (user == null)
        {
            var defaultRole = await _db.Roles
                .FirstOrDefaultAsync(r => r.Libelle == "Technicien");
                // ?? await _db.Roles
                //     .FirstOrDefaultAsync(r => r.Libelle == "Consultant")
                // ?? await _db.Roles.FirstOrDefaultAsync();

            if (defaultRole == null)
                return principal;

            user = new Utilisateur
            {
                IdentifiantAd = samAccountName,
                Nom = principal.Identity.Name,
                Prenom = samAccountName,
                Email = $"{samAccountName}@gmail.com",
                Actif = true,
                DateSynchronisation = DateTime.UtcNow,
                IdRole = defaultRole.IdRole,
                UserGuid = Guid.NewGuid()
            };

            _db.Utilisateurs.Add(user);
            await _db.SaveChangesAsync();

            user = await _db.Utilisateurs
                .Include(u => u.IdRoleNavigation)
                .FirstAsync(u => u.IdentifiantAd == samAccountName);
        }

        if (user.IdRoleNavigation != null)
        {
            var identity = (ClaimsIdentity)principal.Identity!;
            identity.AddClaim(new Claim(ClaimTypes.Role, user.IdRoleNavigation.Libelle));
        }

        return principal;
    }
}
