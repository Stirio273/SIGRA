namespace SIGRA.Services;

public class ChartGenerator
{
    // Génère un graphique en barres et retourne l'image en bytes
    public byte[] GenerateWeeklyRequestsBarChart(WeeklyRequestsReportDto report)
    {
        var plot = new ScottPlot.Plot();

        var labels = report.Entries
            .Select(x => $"S{x.WeekNumber}")
            .ToArray();

        var values = report.Entries
            .Select(x => (double)x.Count)
            .ToArray();

        var bars = plot.Add.Bars(values);
        plot.Axes.Bottom.SetTicks(
            Enumerable.Range(0, labels.Length).Select(x => (double)x).ToArray(),
            labels);

        plot.Title("Demandes reçues par semaine");
        plot.Axes.Left.Label.Text = "Nombre de demandes";

        return plot.GetImageBytes(600, 350, ScottPlot.ImageFormat.Png);
    }

    // Génère un camembert (Répartition par application)
    public byte[] GenerateApplicationPieChart(RequestsByApplicationReportDto report)
    {
        var plot = new ScottPlot.Plot();

        var values = report.Entries.Select(x => (double)x.Count).ToArray();
        var labels = report.Entries.Select(x => x.ApplicationName).ToArray();

        var pie = plot.Add.Pie(values);
        for (int i = 0; i < pie.Slices.Count; i++)
        {
            pie.Slices[i].Label = $"{labels[i]} ({report.Entries[i].Percentage:0.0}%)";
        }

        plot.Title("Répartition par application");
        plot.ShowLegend();

        return plot.GetImageBytes(600, 400, ScottPlot.ImageFormat.Png);
    }

    // Génère un graphique en courbes (Évolution SLA)
    public byte[] GenerateSlaLineChart(SlaComplianceReportDto report)
    {
        var plot = new ScottPlot.Plot();

        var xs = report.Entries
            .Select((x, i) => (double)i)
            .ToArray();

        var ys = report.Entries
            .Select(x => x.ComplianceRate)
            .ToArray();

        var labels = report.Entries
            .Select(x => $"S{x.WeekNumber}")
            .ToArray();

        var line = plot.Add.Scatter(xs, ys);
        line.LineWidth = 2;
        line.MarkerSize = 6;

        plot.Axes.Bottom.SetTicks(xs, labels);
        plot.Title("Évolution du respect des SLA");
        plot.Axes.Left.Label.Text = "Taux de conformité (%)";

        // Ligne de seuil à 80%
        var threshold = plot.Add.HorizontalLine(80);
        threshold.LinePattern = ScottPlot.LinePattern.Dashed;
        threshold.Color = ScottPlot.Colors.Red;

        return plot.GetImageBytes(600, 350, ScottPlot.ImageFormat.Png);
    }
}
