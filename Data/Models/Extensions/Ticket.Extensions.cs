using SIGRA.Data.Enums;

namespace SIGRA.Data.Models;

public partial class Ticket
{
    public void Transferer(Escalade transfert)
    {
        if (this.IdStatut == (int)TicketStatus.Closed)
        {
            throw new InvalidOperationException("Cannot transfer a closed ticket.");
        }
        if (transfert.EstDefinitif)
        {
            this.IdStatut = (int)TicketStatus.Solved;
        }
        this.IdStatut = (int)TicketStatus.Redirected;
    }

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