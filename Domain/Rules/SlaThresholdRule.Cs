using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Domain.Rules;

public class SlaThresholdRule : ITicketAlertRule
{
    private const double ThresholdPercentage = 0.8;

    private readonly IBusinessTimeCalculator _businessTimeCalculator;
    // private readonly ISlaPolicyProvider _slaPolicyProvider;

    public SlaThresholdRule(
        IBusinessTimeCalculator businessTimeCalculator)
    // ISlaPolicyProvider slaPolicyProvider)
    {
        _businessTimeCalculator = businessTimeCalculator;
        // _slaPolicyProvider = slaPolicyProvider;
    }

    public string AlertType => "SlaThreshold80Percent";

    public IQueryable<Ticket> GetCandidates(AppDbContext db)
    {
        var now = DateTime.UtcNow;

        // Filtre SQL LARGE mais efficace : élimine déjà les tickets
        // fermés ou dont le délai est déjà (largement) dépassé
        return db.Tickets
            .Where(t => t.IdStatut != (int)TicketStatus.Closed
                     && t.DeadlineResolution > now);
    }

    public async Task<bool> ShouldTriggerAsync(Ticket ticket)
    {
        // var policy = await _slaPolicyProvider.GetPolicyAsync(ticket.Priority);
        var resolutionTime = ticket.DureeSla;

        var elapsed = await _businessTimeCalculator
            .GetElapsedBusinessTimeAsync(ticket.DateCreation, DateTime.UtcNow);

        var thresholdDuration = TimeSpan.FromTicks(
            (long)((double)resolutionTime * ThresholdPercentage));

        return elapsed >= thresholdDuration;
    }

    public string BuildMessage(Ticket ticket) =>
        $"Le ticket #{ticket.NumeroTicket} a atteint 80% de son délai de résolution SLA.";
}
