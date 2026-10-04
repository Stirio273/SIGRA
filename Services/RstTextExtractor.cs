using System.Diagnostics;

namespace SIGRA.Services;

public sealed class RstTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string fileName, string contentType) =>
        fileName.EndsWith(".rst", StringComparison.OrdinalIgnoreCase);

    public async Task<TextExtractionResult> ExtractAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        if (!await IsPandocAvailableAsync(cancellationToken))
            return TextExtractionResult.Fail("Pandoc is not installed or not available on PATH.");

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pandoc",
                    Arguments = "-f rst -t plain",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();

            using var registration = cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill(true);
                }
                catch { }
            });

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            await fileStream.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
            await process.StandardInput.BaseStream.FlushAsync(cancellationToken);
            process.StandardInput.Close();

            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
                return TextExtractionResult.Fail($"Pandoc failed with exit code {process.ExitCode}: {stderr}");

            return string.IsNullOrWhiteSpace(stdout)
                ? TextExtractionResult.Fail("No text content found in RST document.")
                : TextExtractionResult.Ok(stdout);
        }
        catch (Exception ex)
        {
            return TextExtractionResult.Fail($"Pandoc extraction failed: {ex.Message}");
        }
    }

    private static async Task<bool> IsPandocAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pandoc",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
