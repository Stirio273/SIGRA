using System.ComponentModel.DataAnnotations;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;
using SIGRA.Domain;

namespace SIGRA.Controllers;

public class ReopenTicketDto
{
    [Required, MinLength(10, ErrorMessage = "Le motif doit contenir au moins 10 caractères.")]
    public string Justification { get; set; } = default!;
}

public record CreateRoleRequest(string Libelle);
public record UpdateRoleRequest(string Libelle);
public record RoleResponse(int IdRole, string Libelle);

public record CreateApplicationRequest(string Libelle, bool Actif, int IdCs);
public record UpdateApplicationRequest(string Libelle, bool Actif, int IdCs);
public record ApplicationResponse(int IdApplication, string Libelle, bool Actif, int IdCs);

public record CreateClassesServiceRequest(string Code, string? Libelle, decimal DureeSla);
public record UpdateClassesServiceRequest(string Code, string? Libelle, decimal DureeSla, int IdCriticite);
public record ClassesServiceResponse(int IdCs, string Code, string? Libelle, decimal DureeSla, CriticiteRefResponse criticite);

public record CreateCriticiteRequest(string Libelle, int Ordre);
public record UpdateCriticiteRequest(string Libelle, int Ordre);
public record CriticiteResponse(int IdCriticite, string Libelle, int Ordre);

public record CreateEntitesExterneRequest(string Nom, bool Actif);
public record UpdateEntitesExterneRequest(string Nom, bool Actif);
public record EntitesExterneResponse(int IdEntiteExterne, string Nom, bool Actif);

public record CreateJoursFerieRequest(DateOnly Date, string Libelle);
public record UpdateJoursFerieRequest(DateOnly Date, string Libelle);
public record JoursFerieResponse(int IdJourFerie, DateOnly Date, string Libelle);

public record CreateUtilisateurRequest(string Email, int IdRole);
public record UpdateUtilisateurRequest(string IdentifiantAd, string Nom, string Prenom, string Email, int IdRole, bool Actif, DateTime? DateDesactivation);
public record UtilisateurResponse(int IdUtilisateur, string IdentifiantAd, string Nom, string Prenom, string Email, bool Actif, DateTime? DateDesactivation, DateTime DateSynchronisation, int IdRole, Guid UserGuid);
public record TechnicienResponse(string Nom, string Prenom, string Email, Guid UserGuid);

public record CreateTicketRequest(
    int? IdApplication,
    // int? IdTypeDemande,
    int? IdCriticite,
    int IdStatut,
    int? IdTechnicienAssigne,
    string DemandeurEmail,
    string DemandeurDirection,
    decimal DureeSla);

public record UpdateTicketRequest(
    int? IdApplication,
    // int? IdTypeDemande,
    int? IdCriticite,
    int IdStatut,
    int? IdTechnicienAssigne,
    string? DemandeurEmail,
    string? DemandeurDirection,
    DateTime? DateCloture,
    decimal? DureeSla);

public record CreateDenyRequest(
    int IdTicket,
    string Justificatif
);

public record RespondDenyRequest(
    int IdTicket,
    bool decision
);

public record UpdateTicketApplicationRequest(int? IdApplication);

public record TransferTicketRequest(
    int idTicket,
    int idEntiteExterne,
    string explication,
    bool estDefinitif
);

public record AssignTicketsRequest(List<int> TicketIds, Guid? UserGuid);

public record ReassignTicketRequest(List<int> TicketIds, Guid? UserGuid, string justification);

public record PendingRejectResponse(int RejetId, int TicketId, TechnicienResponse Auteur, string Justificatif, DateTime DateProposition, int? IdValidateur, bool? Decision, DateTime? DateDecision);

public record StatutSuivantPossibleResponse(int IdStatut, string Libelle);

public record TicketApplicationResponse(int IdApplication, string Libelle, bool Actif, int IdCs);
public record TicketCriticiteResponse(int IdCriticite, string Libelle, int Ordre);
public record TicketStatutResponse(int IdStatut, string Libelle, bool EstDefaut);
public record TicketTechnicienResponse(int IdUtilisateur, string Nom, string Prenom, string Email, Guid UserGuid);
public record EmailsSourceResponse(string Expediteur, string? Objet, string? CorpsEmail, DateTime DateReception, ICollection<PiecesJointe> PiecesJointes);

public record ApplicationRefResponse(int IdApplication, string Libelle);
public record StatutRefResponse(int IdStatut, string Libelle);
public record CriticiteRefResponse(int IdCriticite, string Libelle);
public record TechnicienRefResponse(int IdUtilisateur, string Email);

public record TicketResponse(
    int IdTicket,
    string NumeroTicket,
    DateTime DateCreation,
    TicketApplicationResponse? Application,
    TicketCriticiteResponse? Criticite,
    TicketStatutResponse Statut,
    TicketTechnicienResponse? TechnicienAssigne,
    string DemandeurEmail,
    string DemandeurDirection,
    DateTime? DateCloture,
    decimal DureeSla,
    DateTime? DeadlineResolution,
    IReadOnlyList<EmailsSourceResponse>? EmailsSources);

public record TicketSearchRequest(PagedRequest Pagination)
{
    // Filtres
    public string? SearchText { get; set; }         // Recherche dans titre/description
    public TicketStatus? Status { get; set; }
    public int? Criticite { get; set; }
    public string? ApplicationName { get; set; }
    public Guid? AssignedTechnician { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    // public bool? IsOverdue { get; set; }

    // Tri
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}


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

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string EventType { get; set; } = default!;
    public Guid? ResourceId { get; set; }
    public string? ResourceType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketExportRequest
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string Format { get; set; } = "csv";
}

public class ReportQueryParameters
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public Result Validate()
    {
        From = From.ToUniversalTime();
        To = To.ToUniversalTime();

        if (From > To)
            return Result.Failure(
                "La date de début doit être antérieure à la date de fin.", ErrorType.BadRequest);

        if ((To - From).TotalDays > 365)
            return Result.Failure(
                "La période ne peut pas dépasser 365 jours.", ErrorType.BadRequest
                );

        return Result.Success();
    }
}

public record TicketExportRow(
    string NumeroTicket,
    string Statut,
    string? Priorite,
    string? Application,
    string? Criticite,
    string Demandeur,
    string Direction,
    string? AssigneA,
    DateTime DateCreation,
    decimal SlaHeures,
    DateTime? DeadlineResolution,
    DateTime? DateCloture,
    string? SujetEmailInitial,
    string? CorpsEmailInitial
);

public class CreateCommentRequest
{
    public string Contenu { get; set; } = default!;
}

public record CommentaireResponse(int IdCommentaire, int IdTicket, int IdAuteur, string AuteurNom, string AuteurPrenom, string Contenu, DateTime DateCreation);

public record AskAIRequest(string Message);


