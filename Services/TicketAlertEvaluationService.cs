using SIGRA.Data;
using SIGRA.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;
using Hangfire;

namespace SIGRA.Services;

public class TicketAlertEvaluationService
{
    private readonly AppDbContext _db;
    private readonly IEnumerable<ITicketAlertRule> _rules;
    private readonly INotificationService _notificationService;
    private readonly ILogger<TicketAlertEvaluationService> _logger;

    public TicketAlertEvaluationService(
        AppDbContext db,
        IEnumerable<ITicketAlertRule> rules,
        INotificationService notificationService,
        ILogger<TicketAlertEvaluationService> logger)
    {
        _db = db;
        _rules = rules;
        _notificationService = notificationService;
        _logger = logger;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task EvaluateAllRulesAsync()
    {
        foreach (var rule in _rules)
        {
            try
            {
                await EvaluateRuleAsync(rule);
            }
            catch (Exception ex)
            {
                // Une règle en erreur ne doit JAMAIS bloquer les autres
                _logger.LogError(ex, "Erreur lors de l'évaluation de la règle {AlertType}", rule.AlertType);
            }
        }
    }

    private async Task EvaluateRuleAsync(ITicketAlertRule rule)
    {
        var candidates = await rule.GetCandidates(_db).ToListAsync();
        var idAdmin = await _db.Utilisateurs.Where(u => u.IdRoleNavigation.Libelle == "Administrateur").Select(u => u.IdUtilisateur).FirstOrDefaultAsync();

        _logger.LogInformation(
            "Règle {AlertType} : {Count} ticket(s) candidat(s)", rule.AlertType, candidates.Count);

        foreach (var ticket in candidates)
        {
            if (!await rule.ShouldTriggerAsync(ticket))
                continue;

            // Vérifier qu'une alerte n'est pas déjà active pour éviter le spam
            var alreadyAlerted = await _db.AlerteTickets.AnyAsync(a =>
                a.IdTicket == ticket.IdTicket
                && a.TypeAlerte == rule.AlertType
                && a.DateExpiration == null);

            if (alreadyAlerted)
                continue;

            var alert = AlerteTicket.Create(ticket.IdTicket, rule.AlertType);
            _db.AlerteTickets.Add(alert);
            await _db.SaveChangesAsync();
            try
            {
                await _notificationService.SendAsync(idAdmin, ticket.IdTicket, "Alerte", rule.BuildMessage(ticket), rule.AlertType);
                await _notificationService.SendAsync(ticket.IdTechnicienAssigne ?? 0, ticket.IdTicket, "Alerte", rule.BuildMessage(ticket), rule.AlertType);
            }
            catch (System.Exception e)
            {
                _logger.LogError("Erreur lors de l'envoi de la notification correspondant a l'alerte {alerte} sur le ticket {numeroTicket} vers l'utilisateur {idDestinataire} : {error}", 
                rule.AlertType, ticket.NumeroTicket, ticket.IdTechnicienAssigne, e.Message);
            }
        }
    }
}
