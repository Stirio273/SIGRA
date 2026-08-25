using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SIGRA.Services;

public class PdfReportGenerator : IPdfReportGenerator
{
    private readonly ChartGenerator _chartGenerator;

    public PdfReportGenerator(ChartGenerator chartGenerator)
    {
        _chartGenerator = chartGenerator;
    }

    public byte[] GenerateWeeklyReport(WeeklyReportDto report)
    {
        // Génère les images des graphiques AVANT de composer le PDF
        var weeklyRequestsChart = _chartGenerator.GenerateWeeklyRequestsBarChart(report.WeeklyRequests);
        var applicationChart = _chartGenerator.GenerateApplicationPieChart(report.RequestsByApplication);
        var slaChart = _chartGenerator.GenerateSlaLineChart(report.SlaCompliance);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().Element(x => ComposeHeader(x, report));

                page.Content().Column(column =>
                {
                    column.Spacing(20);

                    // Graphique en barres
                    column.Item().Column(col =>
                    {
                        col.Item().Text("Demandes reçues par semaine").FontSize(14).Bold();
                        col.Item().PaddingTop(5).Image(weeklyRequestsChart);
                    });

                    // Graphique en camembert
                    column.Item().Column(col =>
                    {
                        col.Item().Text("Répartition par application").FontSize(14).Bold();
                        col.Item().PaddingTop(5).Image(applicationChart);
                    });

                    // Graphique en courbes
                    column.Item().Column(col =>
                    {
                        col.Item().Text("Évolution du respect des SLA").FontSize(14).Bold();
                        col.Item().PaddingTop(5).Image(slaChart);
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, WeeklyReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Text("Rapport Hebdomadaire")
                .FontSize(20).Bold();

            column.Item().Text(
                $"Période du {report.WeekStart:dd/MM/yyyy} au {report.WeekEnd:dd/MM/yyyy}")
                .FontSize(12).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(10).LineHorizontal(1)
                .LineColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeContent(IContainer container, WeeklyReportDto report)
    {
        container.PaddingVertical(15).Column(column =>
        {
            column.Spacing(20);

            // Section 1 — Résumé
            column.Item().Element(x => ComposeSummarySection(x, report));

            // Section 2 — Demandes par semaine
            column.Item().Element(x => ComposeWeeklyRequestsSection(x, report));

            // Section 3 — Répartition par application
            column.Item().Element(x => ComposeApplicationSection(x, report));

            // Section 4 — Respect des SLA
            column.Item().Element(x => ComposeSlaSection(x, report));
        });
    }

    private void ComposeSummarySection(IContainer container, WeeklyReportDto report)
    {
        container.Background(Colors.Grey.Lighten4).Padding(15).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Total demandes").FontSize(10).FontColor(Colors.Grey.Darken1);
                col.Item().Text(report.WeeklyRequests.Total.ToString())
                    .FontSize(24).Bold();
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Taux SLA moyen").FontSize(10).FontColor(Colors.Grey.Darken1);
                col.Item().Text($"{report.SlaCompliance.AverageComplianceRate:0.0}%")
                    .FontSize(24).Bold()
                    .FontColor(report.SlaCompliance.AverageComplianceRate >= 80
                        ? Colors.Green.Darken1
                        : Colors.Red.Darken1);
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Applications").FontSize(10).FontColor(Colors.Grey.Darken1);
                col.Item().Text(report.RequestsByApplication.Entries.Count.ToString())
                    .FontSize(24).Bold();
            });
        });
    }

    private void ComposeWeeklyRequestsSection(IContainer container, WeeklyReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Text("Demandes reçues par semaine").FontSize(14).Bold();

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeaderStyle).Text("Semaine");
                    header.Cell().Element(CellHeaderStyle).Text("Date de début");
                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("Nombre");
                });

                foreach (var entry in report.WeeklyRequests.Entries)
                {
                    table.Cell().Element(CellStyle).Text($"S{entry.WeekNumber}");
                    table.Cell().Element(CellStyle).Text(entry.WeekStart.ToString("dd/MM/yyyy"));
                    table.Cell().Element(CellStyle).AlignRight().Text(entry.Count.ToString());
                }
            });
        });
    }

    private void ComposeApplicationSection(IContainer container, WeeklyReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Text("Répartition par application").FontSize(14).Bold();

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeaderStyle).Text("Application");
                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("Nombre");
                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("%");
                });

                foreach (var entry in report.RequestsByApplication.Entries)
                {
                    table.Cell().Element(CellStyle).Text(entry.ApplicationName);
                    table.Cell().Element(CellStyle).AlignRight().Text(entry.Count.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text($"{entry.Percentage:0.0}%");
                }
            });
        });
    }

    private void ComposeSlaSection(IContainer container, WeeklyReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Text("Évolution du respect des SLA").FontSize(14).Bold();

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeaderStyle).Text("Semaine");
                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("Total");
                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("Conformes");
                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("Taux");
                });

                foreach (var entry in report.SlaCompliance.Entries)
                {
                    table.Cell().Element(CellStyle).Text($"S{entry.WeekNumber}");
                    table.Cell().Element(CellStyle).AlignRight().Text(entry.TotalCount.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text(entry.CompliantCount.ToString());

                    table.Cell().Element(CellStyle).AlignRight().Text(text =>
                    {
                        text.Span($"{entry.ComplianceRate:0.0}%")
                            .FontColor(entry.ComplianceRate >= 80
                                ? Colors.Green.Darken1
                                : Colors.Red.Darken1);
                    });
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Généré automatiquement le ").FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span(DateTime.Now.ToString("dd/MM/yyyy à HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
        });
    }

    private static IContainer CellHeaderStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Darken2)
            .Padding(6)
            .DefaultTextStyle(x => x.FontColor(Colors.White).Bold());
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(6);
    }

    public Task<byte[]> GenerateFromHtmlAsync(string html)
    {
        throw new NotImplementedException();
    }
}
