using SIGRA.Data.Models;
using SIGRA.Domain.Exceptions;

namespace SIGRA.Services;

public interface ICommentaireService
{
    Task<IReadOnlyList<Commentaire>> GetByTicketIdAsync(int ticketId);
    Task<Commentaire> AddAsync(int ticketId, int idAuteur, string contenu);
}
