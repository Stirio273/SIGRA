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
        var validation = new ReportQueryParameters { From = from, To = to }.Validate();

        if (!validation.IsSuccess)
            return validation.ToHttpResult();

        var result = await _reportService.GetWeeklyRequestsAsync(from, to);

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
        var validation = new ReportQueryParameters { From = from, To = to }.Validate();

        if (!validation.IsSuccess)
            return validation.ToHttpResult();

        var result = await _reportService.GetRequestsByApplicationAsync(from, to);

        // if (!result.IsSuccess)
        //     return result.ToHttpResult();

        return Ok(result);
    }

    // Évolution du respect des SLA
    [HttpGet("sla-compliance")]
    public async Task<IActionResult> GetSlaCompliance(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var validation = new ReportQueryParameters { From = from, To = to }.Validate();

        if (!validation.IsSuccess)
            return validation.ToHttpResult();

        var result = await _reportService.GetSlaComplianceAsync(from, to);

        // if (!result.IsSuccess)
        //     return result.ToHttpResult();

        return Ok(result);
    }
}