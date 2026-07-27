using SIGRA.Data.Enums;

namespace SIGRA.Data.Models;

public partial class Ticket
{
    public Rejet Rejeter(Rejet rejet, int idValidateur)
    {
        this.IdStatut = (int)TicketStatus.Rejected;
        rejet.IdValidateur = idValidateur;
        rejet.Decision = true;
        rejet.DateDecision = DateTime.Now;
        return rejet;
    }

    public Rejet RefuserRejet(Rejet rejet, int idValidateur)
    {
        this.IdStatut = (int)TicketStatus.New;
        rejet.IdValidateur = idValidateur;
        rejet.Decision = false;
        rejet.DateDecision = DateTime.Now;
        return rejet;
    }
}