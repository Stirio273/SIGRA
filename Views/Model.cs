public record WeeklyReportViewModel
{
    public required string WeekStartFormatted { get; init; }
    public required string WeekEndFormatted { get; init; }

    public required int TotalTicketsCreated { get; init; }
    public required int TotalTicketsClosed { get; init; }
    public required double SlaRespectedPercent { get; init; }
    public required double? MeanResolutionTimeHours { get; init; }

    public required string ChartDataJson { get; init; }

    public required int ApplicationCount { get; init; }
    public required string TopApplication { get; init; }
    public required int TopApplicationCount { get; init; }

    public required double? LastWeekSlaRate { get; init; }
    public required int? LastWeekTicketCount { get; init; }
    public required double? PreviousWeekSlaRate { get; init; }
    public required int? PreviousWeekTicketCount { get; init; }
    public required double? SlaRateEvolution { get; init; }
    public required int? TicketCountEvolution { get; init; }
}
