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
        var tickets = await _context.Tickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from && x.DateCreation <= to)
            .Select(x => new { x.DateCreation })
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
                Count = g.Count()
            })
            .OrderBy(x => x.WeekStart)
            .ToList();

        var weeklyRequestsReport = new WeeklyRequestsReportDto
        {
            From = from,
            To = to,
            Entries = entries,
            Total = tickets.Count
        };

        return weeklyRequestsReport;
    }

    // Répartition par application
    public async Task<RequestsByApplicationReportDto> GetRequestsByApplicationAsync(
        DateTime from,
        DateTime to)
    {
        var total = await _context.Tickets
            .AsNoTracking()
            .CountAsync(x => x.DateCreation >= from && x.DateCreation <= to);

        var entries = await _context.Tickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from && x.DateCreation <= to)
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
        DateTime to)
    {
        var tickets = await _context.Tickets
            .AsNoTracking()
            .Where(x => x.DateCreation >= from && x.DateCreation <= to)
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
}