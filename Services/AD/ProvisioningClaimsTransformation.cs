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
        var samAccountName = principal.Identity!.Name!.Split('\\')[1];
        var existe = await _db.Utilisateurs.AnyAsync(u => u.IdentifiantAd == samAccountName); 

        if(!existe)
        {
            _db.Utilisateurs.Add(new Utilisateur
            {
                IdentifiantAd = samAccountName,
                Nom = principal.Identity.Name               
            });
            await _db.SaveChangesAsync();
        }

        return principal;
    }
}