public record WeeklyReportViewModel
{
    public required string WeekStartFormatted { get; init; }
    public required string WeekEndFormatted { get; init; }

    public required int TotalTicketsCreated { get; init; }
    public required int TotalTicketsClosed { get; init; }
    public required double SlaRespectedPercent { get; init; }

    public required string ChartDataJson { get; init; }   // Sérialisé une seule fois, injecté tel quel
}
