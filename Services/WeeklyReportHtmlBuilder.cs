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
        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(Path.Combine(AppContext.BaseDirectory, "wwwroot", "report-templates"))
            .UseMemoryCachingProvider()
            .Build();

        _chartJsContent = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "wwwroot", "report-templates", "chart.umd.min.js"));
    }

    public async Task<string> BuildAsync(WeeklyReportViewModel model)
    {
        var html = await _engine.CompileRenderAsync("WeeklyReport.cshtml", model);

        // Remplace la référence externe par le contenu INLINE du script
        return html.Replace(
            "<script src=\"chart.umd.min.js\"></script>",
            $"<script>{_chartJsContent}</script>");
    }
}
