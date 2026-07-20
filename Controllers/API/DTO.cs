namespace SIGRA.Controllers;

public record CreateApplicationRequest(string Libelle, bool Actif, int IdCs);
public record UpdateApplicationRequest(string Libelle, bool Actif, int IdCs);
public record ApplicationResponse(int IdApplication, string Libelle, bool Actif, int IdCs);

public record CreateClassesServiceRequest(string Code, string? Libelle);
public record UpdateClassesServiceRequest(string Code, string? Libelle);
public record ClassesServiceResponse(int IdCs, string Code, string? Libelle);

public record CreateCriticiteRequest(string Libelle, int Ordre);
public record UpdateCriticiteRequest(string Libelle, int Ordre);
public record CriticiteResponse(int IdCriticite, string Libelle, int Ordre);

public record CreateEntitesExterneRequest(string Nom, bool Actif);
public record UpdateEntitesExterneRequest(string Nom, bool Actif);
public record EntitesExterneResponse(int IdEntiteExterne, string Nom, bool Actif);

public record CreateJoursFerieRequest(DateOnly Date, string Libelle);
public record UpdateJoursFerieRequest(DateOnly Date, string Libelle);
public record JoursFerieResponse(int IdJourFerie, DateOnly Date, string Libelle);

public record CreateTicketRequest(
    int? IdApplication,
    int? IdTypeDemande,
    int? IdCriticite,
    int IdStatut,
    int? IdTechnicienAssigne,
    string DemandeurEmail,
    string DemandeurDirection,
    decimal DureeSla);

public record UpdateTicketRequest(
    int? IdApplication,
    int? IdTypeDemande,
    int? IdCriticite,
    int IdStatut,
    int? IdTechnicienAssigne,
    string DemandeurEmail,
    string DemandeurDirection,
    DateTime? DateCloture,
    decimal DureeSla);

public record TicketResponse(
    int IdTicket,
    string NumeroTicket,
    DateTime DateCreation,
    int? IdApplication,
    int? IdTypeDemande,
    int? IdCriticite,
    int IdStatut,
    int? IdTechnicienAssigne,
    string DemandeurEmail,
    string DemandeurDirection,
    DateTime? DateCloture,
    decimal DureeSla);

public class PagedRequest
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = new List<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
