using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class EmailsSource
{
    public int IdEmailSource { get; set; }

    public int IdTicket { get; set; }

    public string MessageIdGraph { get; set; } = null!;

    public string ConversationIdGraph { get; set; } = null!;

    public string Expediteur { get; set; } = null!;

    public string? Objet { get; set; }

    public string? CorpsEmail { get; set; }

    public DateTime DateReception { get; set; }

    public bool EstEmailInitial { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;

    public virtual ICollection<PiecesJointe> PiecesJointes { get; set; } = new List<PiecesJointe>();
}
