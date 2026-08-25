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
            .Where(x => x.DateCreation >= from.ToUniversalTime() && x.DateCreation <= to.ToUniversalTime())
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
            .CountAsync(x => x.DateCreation >= from.ToUniversalTime() && x.DateCreation <= to.ToUniversalTime());

        var entries = await _context.Tickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from.ToUniversalTime() && x.DateCreation <= to.ToUniversalTime() && x.IdApplication != null)
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
            .Where(x => x.DateCreation >= from.ToUniversalTime() && x.DateCreation <= to.ToUniversalTime());

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
            .Where(x => x.DateCreation >= from.ToUniversalTime() && x.DateCreation <= to.ToUniversalTime() && x.DateCloture != null)
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

        var previousWeekStart = currentWeekStart.AddDays(-7);
        var twoWeeksAgoStart = currentWeekStart.AddDays(-14);

        var from = twoWeeksAgoStart.ToUniversalTime();
        var to = previousWeekStart.AddDays(6).ToUniversalTime();

        var tickets = await _context.RealTickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from && x.DateCreation <= to)
            .Select(x => new
            {
                Week = ISOWeek.GetWeekOfYear(x.DateCreation),
                Year = ISOWeek.GetYear(x.DateCreation),
                x.DateCloture,
                x.DeadlineResolution
            })
            .ToListAsync();

        var grouped = tickets
            .GroupBy(x => new { x.Week, x.Year })
            .ToDictionary(g => (g.Key.Week, g.Key.Year), g => new
            {
                Count = g.Count(),
                SlaReached = g.Count(x => x.DateCloture != null && x.DateCloture <= x.DeadlineResolution)
            });

        static int WeekNumber(DateTime dt) => ISOWeek.GetWeekOfYear(dt);
        static int WeekYear(DateTime dt) => ISOWeek.GetYear(dt);

        var weekA = (WeekNumber(twoWeeksAgoStart), WeekYear(twoWeeksAgoStart));
        var weekB = (WeekNumber(previousWeekStart), WeekYear(previousWeekStart));

        var entries = new List<LastTwoWeeksEntryDto>();

        foreach (var week in new[] { weekA, weekB })
        {
            var weekStart = ISOWeek.ToDateTime(week.Item2, week.Item1, DayOfWeek.Monday);

            if (grouped.TryGetValue(week, out var data))
            {
                entries.Add(new LastTwoWeeksEntryDto
                {
                    WeekNumber = week.Item1,
                    Year = week.Item2,
                    WeekStart = weekStart,
                    Count = data.Count,
                    SlaRate = data.Count == 0
                        ? 0
                        : Math.Round((double)data.SlaReached / data.Count * 100, 2)
                });
            }
            else
            {
                entries.Add(new LastTwoWeeksEntryDto
                {
                    WeekNumber = week.Item1,
                    Year = week.Item2,
                    WeekStart = weekStart,
                    Count = 0,
                    SlaRate = 0
                });
            }
        }

        var lastWeekEntry = entries[^1];
        var previousWeekEntry = entries[^2];

        var slaRateEvolution = Math.Round(lastWeekEntry.SlaRate - previousWeekEntry.SlaRate, 2);
        var ticketCountEvolution = lastWeekEntry.Count - previousWeekEntry.Count;

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