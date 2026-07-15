using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Notification
{
    public int IdNotification { get; set; }

    public int IdDestinataire { get; set; }

    public int IdTicket { get; set; }

    public int IdTypeEvenement { get; set; }

    public DateTime DateCreation { get; set; }

    public bool EstLue { get; set; }

    public DateTime? DateLecture { get; set; }

    public virtual Utilisateur IdDestinataireNavigation { get; set; } = null!;

    public virtual Ticket IdTicketNavigation { get; set; } = null!;

    public virtual TypesEvenementNotification IdTypeEvenementNavigation { get; set; } = null!;
}
