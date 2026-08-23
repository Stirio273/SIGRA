using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface ICommentaireRepository
{
    Task AddAsync(Commentaire commentaire, CancellationToken ct = default);
    Task<IReadOnlyList<Commentaire>> GetByTicketIdAsync(int ticketId, CancellationToken ct = default);
}
