using SIGRA.Data.Models;

namespace SIGRA.Services;

public class TicketSlaService : ITicketSlaService
{
    private readonly IBusinessTimeCalculator _businessTimeCalculator;
    // private readonly ISlaPolicyProvider _slaPolicyProvider;

    public TicketSlaService(
        IBusinessTimeCalculator businessTimeCalculator)
    {
        _businessTimeCalculator = businessTimeCalculator;
        // _slaPolicyProvider = slaPolicyProvider;
    }

    public async Task<DateTime> CalculateSlaAsync(Ticket ticket)
    {
        // var policy = await _slaPolicyProvider.GetPolicyAsync(ticket.Priority);
        var resolutionTime = TimeSpan.FromHours((double)ticket.IdApplicationNavigation.IdCsNavigation.DureeSla);

        // Deadlines calculées à partir de la création, en temps ouvré
        // var responseDeadline = await _businessTimeCalculator
        //     .AddBusinessTimeAsync(ticket.DateCreation, policy.ResponseTime);

        var resolutionDeadline = await _businessTimeCalculator
            .AddBusinessTimeAsync(ticket.DateCreation, resolutionTime);

        var now = DateTime.UtcNow;

        // Si le ticket est fermé, on mesure jusqu'à sa fermeture (pas jusqu'à "now")
        var referenceEnd = ticket.DateCloture ?? now;

        var elapsed = await _businessTimeCalculator
            .GetElapsedBusinessTimeAsync(ticket.DateCreation, referenceEnd);

        var remaining = resolutionTime - elapsed;
        if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;

        // return new TicketSlaInfo
        // {
        //     ResponseDeadline = responseDeadline,
        //     ResolutionDeadline = resolutionDeadline,
        //     IsResponseOverdue = ticket.FirstResponseAt == null && now > responseDeadline,
        //     IsResolutionOverdue = ticket.ClosedAt == null && now > resolutionDeadline,
        //     ElapsedBusinessTime = elapsed,
        //     RemainingBusinessTime = remaining
        // };

        return resolutionDeadline;
    }
}