using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Role
{
    public int IdRole { get; set; }

    public string Libelle { get; set; } = null!;

    public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
}
