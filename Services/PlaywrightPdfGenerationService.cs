using Microsoft.Playwright;

namespace SIGRA.Services;


public class PlaywrightPdfGenerationService : IPdfReportGenerator, IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    private async Task EnsureBrowserAsync()
    {
        if (_browser is not null) return;

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
    }

    public async Task<byte[]> GenerateFromHtmlAsync(string html)
    {
        await EnsureBrowserAsync();

        var page = await _browser!.NewPageAsync();
        try
        {
            // Nécessaire pour que le <script src="chart.umd.min.js">
            // relatif puisse être résolu — on définit une base URL locale
            // var baseUri = new Uri(Path.Combine(AppContext.BaseDirectory, "wwwroot", "report-templates") + Path.DirectorySeparatorChar);
            await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });

            await page.WaitForFunctionAsync("() => window.chartsReady === true", new PageWaitForFunctionOptions
            {
                Timeout = 10000
            });
            await page.WaitForTimeoutAsync(300);

            return await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin { Top = "20px", Bottom = "20px", Left = "20px", Right = "20px" }
            });
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null) await _browser.DisposeAsync();
        _playwright?.Dispose();
    }

    public byte[] GenerateWeeklyReport(WeeklyReportDto report)
    {
        throw new NotImplementedException();
    }
}

