using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface ITicketRepository
{
    Task<Ticket> CreateAsync(Ticket ticket, CancellationToken ct = default);
    Task UpdateAsync(Ticket ticket, CancellationToken ct = default);
    Task<EmailSource> CreateEmailSourceAsync(EmailSource emailSource, CancellationToken ct = default);
    Task CreatePieceJointeAsync(PieceJointe pieceJointe, CancellationToken ct = default);
    Task<int?> GetFirstActiveApplicationIdAsync(CancellationToken ct = default);
    Task<int?> GetTypeDemandeIdAsync(string libelle, CancellationToken ct = default);
    Task<int?> GetCriticiteIdAsync(string libelle, CancellationToken ct = default);
    Task<int?> GetStatutIdAsync(string libelle, CancellationToken ct = default);
}
