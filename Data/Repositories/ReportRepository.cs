using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportRepository> _logger;

    public ReportRepository(AppDbContext context, ILogger<ReportRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Demandes par semaine
    public async Task<WeeklyRequestsReportDto> GetWeeklyRequestsAsync(
        DateTime from,
        DateTime to)
    {
        var tickets = await _context.RealTickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from && x.DateCreation <= to)
            .Select(x => new
            {
                x.DateCreation,
                x.DateCloture,
                x.DeadlineResolution
            })
            .ToListAsync();

        // Groupement par semaine en mémoire
        var entries = tickets
            .GroupBy(x => new
            {
                Week = ISOWeek.GetWeekOfYear(x.DateCreation),
                Year = ISOWeek.GetYear(x.DateCreation)
            })
            .Select(g => new WeeklyRequestsEntryDto
            {
                WeekNumber = g.Key.Week,
                Year = g.Key.Year,
                WeekStart = ISOWeek.ToDateTime(g.Key.Year, g.Key.Week, DayOfWeek.Monday),
                Count = g.Count(),
                SlaReachedCount = g.Count(x => x.DateCloture != null && x.DateCloture <= x.DeadlineResolution)
            })
            .OrderBy(x => x.WeekStart)
            .ToList();

        var weeklyRequestsReport = new WeeklyRequestsReportDto
        {
            From = from,
            To = to,
            Entries = entries,
            Total = tickets.Count,
            TotalSlaReached = tickets.Count(x => x.DateCloture != null && x.DateCloture <= x.DeadlineResolution)
        };

        return weeklyRequestsReport;
    }

    // Répartition par application
    public async Task<RequestsByApplicationReportDto> GetRequestsByApplicationAsync(
        DateTime from,
        DateTime to)
    {
        var total = await _context.RealTickets
            .AsNoTracking()
            .CountAsync(x => x.DateCreation >= from && x.DateCreation <= to);

        var entries = await _context.Tickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from && x.DateCreation <= to && x.IdApplication != null)
            .GroupBy(x => new
            {
                x.IdApplicationNavigation.IdApplication,
                x.IdApplicationNavigation.Libelle
            })
            .Select(g => new RequestsByApplicationEntryDto
            {
                ApplicationId = g.Key.IdApplication,
                ApplicationName = g.Key.Libelle,
                Count = g.Count(),
                // Pourcentage calculé en base
                Percentage = total == 0 ? 0 : Math.Round((double)g.Count() / total * 100, 2)
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var requestsByApplicationReport = new RequestsByApplicationReportDto
        {
            From = from,
            To = to,
            Total = total,
            Entries = entries
        };

        return requestsByApplicationReport;
    }

    // Évolution du respect des SLA
    public async Task<SlaComplianceReportDto> GetSlaComplianceAsync(
        DateTime from,
        DateTime to,
        int? idClasseService = null)
    {
        var query = _context.RealTickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from && x.DateCreation <= to);

        if (idClasseService.HasValue)
            query = query.Where(x => x.IdApplicationNavigation.IdCs == idClasseService.Value);

        var tickets = await query
            .Select(x => new
            {
                x.DateCreation,
                // SLA respecté si le ticket a été résolu dans les délais
                IsCompliant = x.DateCloture != null && x.DateCloture <= x.DateCreation.Add(TimeSpan.FromHours((double)x.DureeSla))
            })
            .ToListAsync();

        var entries = tickets
            .GroupBy(x => new
            {
                Week = ISOWeek.GetWeekOfYear(x.DateCreation),
                Year = ISOWeek.GetYear(x.DateCreation)
            })
            .Select(g => new SlaComplianceEntryDto
            {
                WeekNumber = g.Key.Week,
                Year = g.Key.Year,
                WeekStart = ISOWeek.ToDateTime(g.Key.Year, g.Key.Week, DayOfWeek.Monday),
                TotalCount = g.Count(),
                CompliantCount = g.Count(x => x.IsCompliant),
                NonCompliantCount = g.Count(x => !x.IsCompliant),
                ComplianceRate = g.Count() == 0
                    ? 0
                    : Math.Round((double)g.Count(x => x.IsCompliant) / g.Count() * 100, 2)
            })
            .OrderBy(x => x.WeekStart)
            .ToList();

        var averageComplianceRate = entries.Count == 0
            ? 0
            : Math.Round(entries.Average(x => x.ComplianceRate), 2);

        var slaComplianceReport = new SlaComplianceReportDto
        {
            From = from,
            To = to,
            Entries = entries,
            AverageComplianceRate = averageComplianceRate
        };

        return slaComplianceReport;
    }

    // Temps moyen de résolution
    public async Task<MeanResolutionTimeDto> GetMeanResolutionTimeAsync(
        DateTime from,
        DateTime to)
    {
        var closedTickets = await _context.RealTickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from && x.DateCreation <= to && x.DateCloture != null)
            .Select(x => new
            {
                Duration = x.DateCloture.Value - x.DateCreation
            })
            .ToListAsync();

        var meanTime = closedTickets.Count == 0
            ? 0
            : closedTickets.Average(x => x.Duration.TotalHours);

        return new MeanResolutionTimeDto
        {
            MeanTime = Math.Round(meanTime, 2)
        };
    }

    public async Task<LastTwoWeeksReportDto> GetLastTwoWeeksAsync()
    {
        var now = DateTime.UtcNow;
        var currentYear = ISOWeek.GetYear(now);
        var currentWeek = ISOWeek.GetWeekOfYear(now);
        var currentWeekStart = ISOWeek.ToDateTime(currentYear, currentWeek, DayOfWeek.Monday);

        var from = currentWeekStart.AddDays(-14).ToUniversalTime();
        var to = currentWeekStart.AddDays(-1).ToUniversalTime();

        var query = _context.RealTickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from && x.DateCreation <= to);

        var tickets = await query
            .Select(x => new
            {
                Week = ISOWeek.GetWeekOfYear(x.DateCreation),
                Year = ISOWeek.GetYear(x.DateCreation),
                x.DateCloture,
                x.DeadlineResolution
            })
            .ToListAsync();

        var entries = tickets
            .GroupBy(x => new { x.Week, x.Year })
            .Select(g => new LastTwoWeeksEntryDto
            {
                WeekNumber = g.Key.Week,
                Year = g.Key.Year,
                WeekStart = ISOWeek.ToDateTime(g.Key.Year, g.Key.Week, DayOfWeek.Monday),
                Count = g.Count(),
                SlaRate = g.Count() == 0
                    ? 0
                    : Math.Round((double)g.Count(x => x.DateCloture != null && x.DateCloture <= x.DeadlineResolution) / g.Count() * 100, 2)
            })
            .OrderBy(x => x.WeekStart)
            .ToList();

        double? slaRateEvolution = null;
        int? ticketCountEvolution = null;

        if (entries.Count >= 2)
        {
            var lastWeek = entries[entries.Count - 1];
            var previousWeekEntry = entries[entries.Count - 2];

            slaRateEvolution = Math.Round(lastWeek.SlaRate - previousWeekEntry.SlaRate, 2);
            ticketCountEvolution = lastWeek.Count - previousWeekEntry.Count;
        }

        return new LastTwoWeeksReportDto
        {
            From = from,
            To = to,
            Entries = entries,
            SlaRateEvolution = slaRateEvolution,
            TicketCountEvolution = ticketCountEvolution
        };
    }
}