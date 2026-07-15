using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Sla
{
    public int IdSla { get; set; }

    public int IdCs { get; set; }

    public int IdTypeDemande { get; set; }

    public decimal Duree { get; set; }

    public virtual ClassesService IdCsNavigation { get; set; } = null!;

    public virtual TypesDemande IdTypeDemandeNavigation { get; set; } = null!;
}
