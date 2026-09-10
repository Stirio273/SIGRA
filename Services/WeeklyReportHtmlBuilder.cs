using System.IO;
using RazorLight;

namespace SIGRA.Services;

public interface IWeeklyReportHtmlBuilder
{
    Task<string> BuildAsync(WeeklyReportViewModel model);
}

public class WeeklyReportHtmlBuilder : IWeeklyReportHtmlBuilder
{
    private readonly RazorLightEngine _engine;
    private readonly string _chartJsContent;
    private const string TemplateKey = "WeeklyReport";

    public WeeklyReportHtmlBuilder()
    {
        var templatesPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "report-templates");

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatesPath)
            .UseMemoryCachingProvider()
            .Build();

        _chartJsContent = File.ReadAllText(
            Path.Combine(templatesPath, "chart.umd.min.js"));
    }

    public async Task<string> BuildAsync(WeeklyReportViewModel model)
    {
        var html = await _engine.CompileRenderAsync("WeeklyReport.cshtml", model);

        html = html.Replace(
            "<script src=\"chart.umd.min.js\"></script>",
            $"<script>{_chartJsContent}</script>");

        return html.Replace("__CHART_DATA_JSON__", model.ChartDataJson);
    }
}
