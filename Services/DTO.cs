public class ReopenTicketRequest
{
    public int TicketId { get; set; }
    public string Reason { get; set; } = default!;
}


public class WeeklyRequestsReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<WeeklyRequestsEntryDto> Entries { get; set; } = new();
    public int Total { get; set; }
}

public class WeeklyRequestsEntryDto
{
    // Numéro de semaine (ex: 42)
    public int WeekNumber { get; set; }

    // Année (ex: 2024)
    public int Year { get; set; }

    // Date de début de la semaine
    public DateTime WeekStart { get; set; }

    // Nombre de tickets
    public int Count { get; set; }
}

public class RequestsByApplicationReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int Total { get; set; }
    public List<RequestsByApplicationEntryDto> Entries { get; set; } = new();
}

public class RequestsByApplicationEntryDto
{
    public int ApplicationId { get; set; }
    public string ApplicationName { get; set; } = default!;
    public int Count { get; set; }

    // Pourcentage calculé
    public double Percentage { get; set; }
}

public class SlaComplianceReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<SlaComplianceEntryDto> Entries { get; set; } = new();

    // Moyenne globale sur la période
    public double AverageComplianceRate { get; set; }
}

public class SlaComplianceEntryDto
{
    public int WeekNumber { get; set; }
    public int Year { get; set; }
    public DateTime WeekStart { get; set; }

    // Nombre total de tickets
    public int TotalCount { get; set; }

    // Tickets respectant le SLA
    public int CompliantCount { get; set; }

    // Tickets ne respectant pas le SLA
    public int NonCompliantCount { get; set; }

    // Taux de respect en pourcentage
    public double ComplianceRate { get; set; }
}

public class WeeklyReportDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }

    public WeeklyRequestsReportDto WeeklyRequests { get; set; } = default!;
    public RequestsByApplicationReportDto RequestsByApplication { get; set; } = default!;
    public SlaComplianceReportDto SlaCompliance { get; set; } = default!;
}

