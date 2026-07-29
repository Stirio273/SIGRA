using SIGRA.Data.Enums;
using SIGRA.Domain;

namespace SIGRA.Data.Models;

public partial class Ticket
{
    public Result ReassignTo(int? newAssigneeId, string justification)
    {
        if (IdTechnicienAssigne is null)
            return Result.Failure("Ticket is not assigned yet. Use assign instead.", ErrorType.Conflict);

        if (string.IsNullOrWhiteSpace(justification))
            return Result.Failure("Justification is required for reassignment.", ErrorType.Conflict);

        IdTechnicienAssigne = newAssigneeId;
        return Result.Success();
    }


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

    public Rejet ValiderRejet(Rejet rejet, int idValidateur, bool isRejected)
    {
        if (isRejected)
        {
            this.IdStatut = (int)TicketStatus.Rejected;
        }
        else
        {
            this.IdStatut = (int)TicketStatus.New;
        }
        rejet.IdValidateur = idValidateur;
        rejet.Decision = isRejected;
        rejet.DateDecision = DateTime.Now;
        return rejet;
    }
}