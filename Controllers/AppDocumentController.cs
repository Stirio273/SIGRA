using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGRA.Controllers.Helper;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/application-documents")]
public class AppDocumentController : ControllerBase
{
    private readonly TextExtractionService _textExtractionService;
    private readonly AppDocumentService _appDocumentService;
    private readonly IStorageService _storageService;


    public AppDocumentController(TextExtractionService textExtractionService, AppDocumentService appDocumentService, IStorageService storageService)
    {
        _textExtractionService = textExtractionService;
        _appDocumentService = appDocumentService;
        _storageService = storageService;
    }

    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> AddDocument(
    [FromForm] AddAppDocumentForm form,
    CancellationToken cancellationToken)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest("A file is required.");

        var extractionResult = await _textExtractionService.ExtractAsync(
            form.File, cancellationToken);

        if (!extractionResult.Success)
        {
            return UnprocessableEntity(new
            {
                error = "Could not extract text from this file.",
                reason = extractionResult.FailureReason
            });
        }

        var fileUrl = await _storageService.UploadAsync(form.File, "application-documents");

        var document = new AppDocument
        {
            Titre = string.IsNullOrWhiteSpace(form.Title)
            ? Path.GetFileNameWithoutExtension(form.File.FileName)
            : form.Title,
            Contenu = extractionResult.PlainText!,
            NomFichier = form.File.FileName,
            Chemin = fileUrl,
            IdApplication = form.IdApplication,
        };

        await _appDocumentService.AddApplicationDocument(document);

        return Ok(new { document.Id });
    }

    [HttpGet("{idApplication:int}")]
    public async Task<IActionResult> GetAll(int idApplication, CancellationToken cancellationToken)
    {
        var documents = await _appDocumentService.GetAllApplicationDocuments(idApplication);

        return Ok(documents);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _appDocumentService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("download/{idAppDocument:int}")]
    public async Task<IActionResult> GetFileAsync(int idAppDocument)
    {
        var appDocument = await _appDocumentService.GetByIdAsync(idAppDocument);
        if (appDocument == null)
            return NotFound();

        var relativePath = appDocument.Chemin.TrimStart('/');
        var stream = await _storageService.DownloadAsync(relativePath);
        var contentType = FileHelper.GetContentType(appDocument.NomFichier);

        return File(stream, contentType);
    }
}