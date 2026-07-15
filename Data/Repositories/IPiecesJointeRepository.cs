using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IPiecesJointeRepository
{
    Task CreatePieceJointeAsync(PiecesJointe pieceJointe, CancellationToken ct = default);
}