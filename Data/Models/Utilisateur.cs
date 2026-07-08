using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGRA.Data.Models;

[Table("utilisateur")]
public class Utilisateur
{
    [Key]
    [Column("id_utilisateur")]
    public int IdUtilisateur { get; set; }

    [Column("identifiant_ad")]
    [Required]
    public string IdentifiantAd { get; set; } = string.Empty;

    [Column("nom")]
    [Required]
    public string Nom { get; set; } = string.Empty;

    [Column("prenom")]
    [Required]
    public string Prenom { get; set; } = string.Empty;

    [Column("email")]
    public string? Email { get; set; }

    [Column("role")]
    public string? Role { get; set; }

    [Column("actif")]
    public bool EstActif { get; set; } = true;

    [Column("date_desactivation")]
    public DateTime DateDesactivation { get; set; }

    [Column("date_synchronisation")]
    public DateTime DateSynchronisation { get; set; }
}
