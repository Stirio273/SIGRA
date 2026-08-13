using SIGRA.Data.Enums;
using SIGRA.Domain;

namespace SIGRA.Data.Models;

public partial class Ticket
{
    private Result EnsureValidTransition(TicketStatus target)
    {
        if (!TicketStatusTransitions.IsValidTransition((TicketStatus)IdStatut, target))
            return Result.Failure(
                $"Transition invalide : impossible de passer de '{IdStatut}' à '{target}'.", ErrorType.Conflict);
        return Result.Success();
    }

    public Result PasserStatutSuivant(TicketStatus target)
    {
        var verification = EnsureValidTransition(target);
        if (verification != Result.Success())
        {
            return verification;
        }
        IdStatut = (int)target;
        return Result.Success();
    }

    public Result Creer()
    {
        IdStatut = (int)TicketStatus.New;
        return Result.Success();
    }

    public Result AttendreRejet()
    {
        IdStatut = (int)TicketStatus.PendingReject;
        return Result.Success();
    }

    public Result Ouvrir(DateTime deadlineResolution)
    {
        IdStatut = (int)TicketStatus.Opened;
        DateCloture = null;
        DeadlineResolution = deadlineResolution;
        return Result.Success();
    }

    public Result Cloturer()
    {
        if (this.IdStatut == (int)TicketStatus.Closed)
        {
            return Result.Failure("Ticket is already closed", ErrorType.Conflict);
        }
        this.IdStatut = (int)TicketStatus.Closed;
        this.DateCloture = DateTime.UtcNow;
        return Result.Success();
    }

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
            this.IdStatut = (int)TicketStatus.Opened;
        }
        rejet.IdValidateur = idValidateur;
        rejet.Decision = isRejected;
        rejet.DateDecision = DateTime.UtcNow;
        return rejet;
    }
}