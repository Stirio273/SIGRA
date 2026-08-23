using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;
using System.Globalization;
using System.Text;

namespace SIGRA.Services;

public class TicketExportService : ITicketExportService
{
    private readonly ITicketRepository _ticketRepository;

    public TicketExportService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<byte[]> ExportTicketsAsync(DateTime? from, DateTime? to, string format)
    {
        var rows = await _ticketRepository.GetTicketsForExportAsync(from, to);

        if (rows.Count == 0)
            return Array.Empty<byte>();

        return format.ToLowerInvariant() switch
        {
            "excel" or "xlsx" => GenerateExcel(rows),
            "csv" => GenerateCsv(rows),
            _ => throw new ArgumentException($"Format d'export non supporté : {format}")
        };
    }

    private static byte[] GenerateExcel(IReadOnlyList<TicketExportRow> rows)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Tickets");

        var headers = new[]
        {
            "N° Ticket", "Statut", "Priorité", "Application", "Criticité",
            "Demandeur", "Direction", "Assigné à", "Date création",
            "SLA (heures)", "Deadline résolution", "Date clôture",
            "Sujet email initial", "Corps email initial"
        };

        for (var col = 0; col < headers.Length; col++)
        {
            var cell = worksheet.Cells[1, col + 1];
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
        }

        for (var rowIdx = 0; rowIdx < rows.Count; rowIdx++)
        {
            var row = rows[rowIdx];
            var r = rowIdx + 2;

            worksheet.Cells[r, 1].Value = row.NumeroTicket;
            worksheet.Cells[r, 2].Value = row.Statut;
            worksheet.Cells[r, 3].Value = row.Priorite;
            worksheet.Cells[r, 4].Value = row.Application;
            worksheet.Cells[r, 5].Value = row.Criticite;
            worksheet.Cells[r, 6].Value = row.Demandeur;
            worksheet.Cells[r, 7].Value = row.Direction;
            worksheet.Cells[r, 8].Value = row.AssigneA;
            worksheet.Cells[r, 9].Value = row.DateCreation.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            worksheet.Cells[r, 10].Value = row.SlaHeures;
            worksheet.Cells[r, 11].Value = row.DeadlineResolution.HasValue
                ? row.DeadlineResolution.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                : null;
            worksheet.Cells[r, 12].Value = row.DateCloture.HasValue
                ? row.DateCloture.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                : null;
            worksheet.Cells[r, 13].Value = row.SujetEmailInitial;
            worksheet.Cells[r, 14].Value = row.CorpsEmailInitial;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    private static byte[] GenerateCsv(IReadOnlyList<TicketExportRow> rows)
    {
        var sb = new StringBuilder();

        var headers = new[]
        {
            "N° Ticket", "Statut", "Priorité", "Application", "Criticité",
            "Demandeur", "Direction", "Assigné à", "Date création",
            "SLA (heures)", "Deadline résolution", "Date clôture",
            "Sujet email initial", "Corps email initial"
        };

        sb.AppendLine(string.Join(";", headers.Select(EscapeCsvField)));

        foreach (var row in rows)
        {
            var cells = new object[]
            {
                row.NumeroTicket,
                row.Statut,
                row.Priorite ?? string.Empty,
                row.Application ?? string.Empty,
                row.Criticite ?? string.Empty,
                row.Demandeur,
                row.Direction,
                row.AssigneA ?? string.Empty,
                row.DateCreation.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                row.SlaHeures,
                row.DeadlineResolution.HasValue ? row.DeadlineResolution.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : string.Empty,
                row.DateCloture.HasValue ? row.DateCloture.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : string.Empty,
                row.SujetEmailInitial ?? string.Empty,
                row.CorpsEmailInitial ?? string.Empty
            };

            sb.AppendLine(string.Join(";", cells.Select(EscapeCsvField)));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsvField(object? value)
    {
        var field = value?.ToString() ?? string.Empty;

        if (field.Contains(';') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            field = field.Replace("\"", "\"\"");
            return $"\"{field}\"";
        }

        return field;
    }
}
