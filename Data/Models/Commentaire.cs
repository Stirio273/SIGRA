using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace SIGRA.Data.Models;

public partial class Commentaire
{
    public int IdCommentaire { get; set; }

    public int IdTicket { get; set; }

    public int IdAuteur { get; set; }

    public string Contenu { get; set; } = null!;

    public DateTime DateCreation { get; set; }

    public bool EstNoteResolution { get; set; }

    public NpgsqlTsVector? ContenuTsv { get; set; }

    public virtual Utilisateur IdAuteurNavigation { get; set; } = null!;

    public virtual Ticket IdTicketNavigation { get; set; } = null!;
}
