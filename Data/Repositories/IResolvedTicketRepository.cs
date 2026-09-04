using SIGRA.Domain.AIsupport;
using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Enums;

namespace SIGRA.Data.Repositories;

public interface IResolvedTicketRepository
{
    Task<IReadOnlyList<ResolvedTicketSummary>> GetCandidatesAsync(
        int idApplication,
        IReadOnlyList<string> keywords,
        // IReadOnlyList<string> allowedModules,
        int? excludeTicketId,
        int maxCandidates,
        CancellationToken cancellationToken = default);
}

public sealed class ResolvedTicketRepository : IResolvedTicketRepository
{
    private readonly AppDbContext _dbContext;

    public ResolvedTicketRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ResolvedTicketSummary>> GetCandidatesAsync(
        int idApplication,
        IReadOnlyList<string> keywords,
        // IReadOnlyList<string> allowedModules,
        int? excludeTicketId,
        int maxCandidates,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tickets
            // .Where(t => t.IdStatut >= (int)TicketStatus.Solved)
            .Where(t => t.EstUtilisableParIA)
            .Where(t => t.IdApplication == idApplication); // scoping decision
                                                           // .Where(t => t.ResolutionNotes != null && t.ResolutionNotes != "");


        if (excludeTicketId is not null)
        {
            query = query.Where(t => t.IdTicket != excludeTicketId);
        }

        // if (allowedModules.Count > 0)
        // {
        //     query = query.Where(t =>
        //         t.CategoryName != null &&
        //         allowedModules.Contains(t.CategoryName));
        // }

        if (keywords.Count > 0)
        {
            query = query.Where(t =>
                keywords.Any(k =>
                    EF.Functions.Like(t.Title, $"%{k}%") ||
                    EF.Functions.Like(t.ResolutionNotes!, $"%{k}%")));
        }

        var results = await query
            .OrderByDescending(t => t.DateCloture)
            .Take(maxCandidates)
            .Select(t => new ResolvedTicketSummary
            {
                IdTicket = t.IdTicket,
                Title = t.Title,
                ResolutionNotes = t.ResolutionNotes!,
                Application = t.IdApplicationNavigation.Libelle,
                // Module = t.CategoryName,
                // ResolutionType = t.ResolutionType,
                // ProblemRecordId = t.ProblemRecordId,
                ResolvedAt = (DateTimeOffset)t.DateCloture
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}
