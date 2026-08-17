namespace SIGRA.Domain;

public record TicketCreatedEvent(int IdTicket);
public record TicketReopenedEvent(
    int TicketId, string Reason, int ReopenedByUserId,
    DateTime OriginalClosedAt, int ReopenCount);

public record TicketReassignedEvent(int IdTicket, string NumeroTicket, int IdTechnicienAssigne);

public record TicketClosedEvent(int TicketId, bool WasSlaBreached);
public record TicketPausedEvent(int TicketId, DateTime PausedAt);
public record TicketResumedEvent(int TicketId, DateTime ResumedAt);