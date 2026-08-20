using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/rapports")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // Demandes par semaine
    [HttpGet("weekly-requests")]
    public async Task<IActionResult> GetWeeklyRequests(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var reportQuery = new ReportQueryParameters { From = from, To = to };
        var validation = reportQuery.Validate();

        if (!validation.IsSuccess)
            return validation.ToHttpResult();

        var result = await _reportService.GetWeeklyRequestsAsync(reportQuery.From, reportQuery.To);

        // if (!result.IsSuccess)
        //     return result.ToHttpResult();

        return Ok(result);
    }

    // Répartition par application
    [HttpGet("requests-by-application")]
    public async Task<IActionResult> GetRequestsByApplication(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var reportQuery = new ReportQueryParameters { From = from, To = to };
        var validation = reportQuery.Validate();

        if (!validation.IsSuccess)
            return validation.ToHttpResult();

        var result = await _reportService.GetRequestsByApplicationAsync(reportQuery.From, reportQuery.To);

        // if (!result.IsSuccess)
        //     return result.ToHttpResult();

        return Ok(result);
    }

    // Évolution du respect des SLA
    [HttpGet("sla-compliance")]
    public async Task<IActionResult> GetSlaCompliance(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? idClasseService = null)
    {
        var reportQuery = new ReportQueryParameters { From = from, To = to };
        var validation = reportQuery.Validate();

        if (!validation.IsSuccess)
            return validation.ToHttpResult();

        var result = await _reportService.GetSlaComplianceAsync(reportQuery.From, reportQuery.To, idClasseService);

        // if (!result.IsSuccess)
        //     return result.ToHttpResult();

        return Ok(result);
    }

    // Temps moyen de résolution
    [HttpGet("mean-resolution-time")]
    public async Task<IActionResult> GetMeanResolutionTime(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var reportQuery = new ReportQueryParameters { From = from, To = to };
        var validation = reportQuery.Validate();

        if (!validation.IsSuccess)
            return validation.ToHttpResult();

        var result = await _reportService.GetMeanResolutionTimeAsync(reportQuery.From, reportQuery.To);

        return Ok(result);
    }

    [HttpGet("last-two-weeks")]
    public async Task<IActionResult> GetLastTwoWeeks()
    {
        var result = await _reportService.GetLastTwoWeeksAsync();

        return Ok(result);
    }
}