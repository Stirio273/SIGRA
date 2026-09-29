using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SIGRA.Controllers.Helper;
using SIGRA.Data.Repositories;
using SIGRA.Domain.Options;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly IPiecesJointeRepository _piecesJointeRepository;

    public FilesController(
        IStorageService storageService,
        IPiecesJointeRepository piecesJointeRepository)
    {
        _storageService = storageService;
        _piecesJointeRepository = piecesJointeRepository;
    }

    [HttpGet("{idPieceJointe:int}")]
    public async Task<IActionResult> GetFileAsync(int idPieceJointe)
    {
        var pieceJointe = await _piecesJointeRepository.GetByIdAsync(idPieceJointe);
        if (pieceJointe == null)
            return NotFound();

        var relativePath = pieceJointe.Chemin.TrimStart('/');
        var stream = await _storageService.DownloadAsync(relativePath);
        var contentType = FileHelper.GetContentType(pieceJointe.NomFichier);

        return File(stream, contentType);
    }

}