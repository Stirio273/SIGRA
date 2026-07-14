using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class RegleCriticite
{
    public int IdRegleCriticite { get; set; }

    public int IdCs { get; set; }

    public int IdTypeDemande { get; set; }

    public int IdCriticite { get; set; }

    public virtual Criticite IdCriticiteNavigation { get; set; } = null!;

    public virtual ClasseDeService IdCsNavigation { get; set; } = null!;

    public virtual TypeDemande IdTypeDemandeNavigation { get; set; } = null!;
}
