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
        var projectRoot = GetProjectRoot();
        var templatesPath = Path.Combine(projectRoot, "wwwroot", "report-templates");

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatesPath)
            .UseMemoryCachingProvider()
            .Build();

        _chartJsContent = File.ReadAllText(
            Path.Combine(templatesPath, "chart.umd.min.js"));
    }

    private static string GetProjectRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "SIGRA.csproj")))
                return directory;
            directory = Directory.GetParent(directory)?.FullName;
        }
        throw new DirectoryNotFoundException("Project root not found");
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
