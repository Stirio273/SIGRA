using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<ClasseDeService> ClasseDeServices { get; set; }

    public virtual DbSet<Commentaire> Commentaires { get; set; }

    public virtual DbSet<Criticite> Criticites { get; set; }

    public virtual DbSet<EmailSource> EmailSources { get; set; }

    public virtual DbSet<EntiteExterne> EntiteExternes { get; set; }

    public virtual DbSet<Escalade> Escalades { get; set; }

    public virtual DbSet<HistoriqueStatut> HistoriqueStatuts { get; set; }

    public virtual DbSet<JourFerie> JourFeries { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PieceJointe> PieceJointes { get; set; }

    public virtual DbSet<Reassignation> Reassignations { get; set; }

    public virtual DbSet<RegleCriticite> RegleCriticites { get; set; }

    public virtual DbSet<Rejet> Rejets { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ServiceAccountToken> ServiceAccountTokens { get; set; }

    public virtual DbSet<Sla> Slas { get; set; }

    public virtual DbSet<Statut> Statuts { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TypeDemande> TypeDemandes { get; set; }

    public virtual DbSet<TypeEvenementNotification> TypeEvenementNotifications { get; set; }

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("oauth_provider", new[] { "google", "microsoft" })
            .HasPostgresExtension("unaccent")
            .HasPostgresExtension("vector");

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.IdApplication).HasName("application_pkey");

            entity.ToTable("application");

            entity.HasIndex(e => e.Actif, "idx_application_actif");

            entity.HasIndex(e => e.IdCs, "idx_application_cs");

            entity.Property(e => e.IdApplication).HasColumnName("id_application");
            entity.Property(e => e.Actif)
                .HasDefaultValue(true)
                .HasColumnName("actif");
            entity.Property(e => e.IdCs).HasColumnName("id_cs");
            entity.Property(e => e.Libelle)
                .HasMaxLength(150)
                .HasColumnName("libelle");

            entity.HasOne(d => d.IdCsNavigation).WithMany(p => p.Applications)
                .HasForeignKey(d => d.IdCs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("application_id_cs_fkey");
        });

        modelBuilder.Entity<ClasseDeService>(entity =>
        {
            entity.HasKey(e => e.IdCs).HasName("classe_de_service_pkey");

            entity.ToTable("classe_de_service");

            entity.HasIndex(e => e.Code, "classe_de_service_code_key").IsUnique();

            entity.Property(e => e.IdCs).HasColumnName("id_cs");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.Libelle)
                .HasMaxLength(100)
                .HasColumnName("libelle");
        });

        modelBuilder.Entity<Commentaire>(entity =>
        {
            entity.HasKey(e => e.IdCommentaire).HasName("commentaire_pkey");

            entity.ToTable("commentaire");

            entity.HasIndex(e => e.ContenuTsv, "idx_commentaire_contenu_tsv").HasMethod("gin");

            entity.HasIndex(e => e.EstNoteResolution, "idx_commentaire_note_resolution").HasFilter("(est_note_resolution = true)");

            entity.HasIndex(e => e.IdTicket, "idx_commentaire_ticket");

            entity.Property(e => e.IdCommentaire).HasColumnName("id_commentaire");
            entity.Property(e => e.Contenu).HasColumnName("contenu");
            entity.Property(e => e.ContenuTsv)
                .HasComputedColumnSql("to_tsvector('french'::regconfig, contenu)", true)
                .HasColumnName("contenu_tsv");
            entity.Property(e => e.DateCreation)
                .HasDefaultValueSql("now()")
                .HasColumnName("date_creation");
            entity.Property(e => e.EstNoteResolution).HasColumnName("est_note_resolution");
            entity.Property(e => e.IdAuteur).HasColumnName("id_auteur");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");

            entity.HasOne(d => d.IdAuteurNavigation).WithMany(p => p.Commentaires)
                .HasForeignKey(d => d.IdAuteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("commentaire_id_auteur_fkey");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.Commentaires)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("commentaire_id_ticket_fkey");
        });

        modelBuilder.Entity<Criticite>(entity =>
        {
            entity.HasKey(e => e.IdCriticite).HasName("criticite_pkey");

            entity.ToTable("criticite");

            entity.HasIndex(e => e.Libelle, "criticite_libelle_key").IsUnique();

            entity.HasIndex(e => e.Ordre, "criticite_ordre_key").IsUnique();

            entity.Property(e => e.IdCriticite).HasColumnName("id_criticite");
            entity.Property(e => e.Libelle)
                .HasMaxLength(50)
                .HasColumnName("libelle");
            entity.Property(e => e.Ordre).HasColumnName("ordre");
        });

        modelBuilder.Entity<EmailSource>(entity =>
        {
            entity.HasKey(e => e.IdEmailSource).HasName("email_source_pkey");

            entity.ToTable("email_source");

            entity.HasIndex(e => e.MessageIdGraph, "email_source_message_id_graph_key").IsUnique();

            entity.HasIndex(e => e.ConversationIdGraph, "idx_email_source_conversation");

            entity.HasIndex(e => e.IdTicket, "idx_email_source_ticket");

            entity.HasIndex(e => e.IdTicket, "uq_email_source_initial")
                .IsUnique()
                .HasFilter("(est_email_initial = true)");

            entity.Property(e => e.IdEmailSource).HasColumnName("id_email_source");
            entity.Property(e => e.ConversationIdGraph)
                .HasMaxLength(255)
                .HasColumnName("conversation_id_graph");
            entity.Property(e => e.CorpsEmail).HasColumnName("corps_email");
            entity.Property(e => e.DateReception).HasColumnName("date_reception");
            entity.Property(e => e.EstEmailInitial).HasColumnName("est_email_initial");
            entity.Property(e => e.Expediteur)
                .HasMaxLength(255)
                .HasColumnName("expediteur");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.MessageIdGraph)
                .HasMaxLength(255)
                .HasColumnName("message_id_graph");
            entity.Property(e => e.Objet)
                .HasMaxLength(500)
                .HasColumnName("objet");

            entity.HasOne(d => d.IdTicketNavigation).WithOne(p => p.EmailSource)
                .HasForeignKey<EmailSource>(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("email_source_id_ticket_fkey");
        });

        modelBuilder.Entity<EntiteExterne>(entity =>
        {
            entity.HasKey(e => e.IdEntiteExterne).HasName("entite_externe_pkey");

            entity.ToTable("entite_externe");

            entity.Property(e => e.IdEntiteExterne).HasColumnName("id_entite_externe");
            entity.Property(e => e.Actif)
                .HasDefaultValue(true)
                .HasColumnName("actif");
            entity.Property(e => e.Nom)
                .HasMaxLength(150)
                .HasColumnName("nom");
        });

        modelBuilder.Entity<Escalade>(entity =>
        {
            entity.HasKey(e => e.IdEscalade).HasName("escalade_pkey");

            entity.ToTable("escalade");

            entity.HasIndex(e => e.IdEntiteExterne, "idx_escalade_entite");

            entity.HasIndex(e => e.IdTicket, "idx_escalade_ticket");

            entity.Property(e => e.IdEscalade).HasColumnName("id_escalade");
            entity.Property(e => e.DateEscalade)
                .HasDefaultValueSql("now()")
                .HasColumnName("date_escalade");
            entity.Property(e => e.EstDefinitif).HasColumnName("est_definitif");
            entity.Property(e => e.Explication).HasColumnName("explication");
            entity.Property(e => e.IdAuteur).HasColumnName("id_auteur");
            entity.Property(e => e.IdEntiteExterne).HasColumnName("id_entite_externe");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");

            entity.HasOne(d => d.IdAuteurNavigation).WithMany(p => p.Escalades)
                .HasForeignKey(d => d.IdAuteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("escalade_id_auteur_fkey");

            entity.HasOne(d => d.IdEntiteExterneNavigation).WithMany(p => p.Escalades)
                .HasForeignKey(d => d.IdEntiteExterne)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("escalade_id_entite_externe_fkey");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.Escalades)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("escalade_id_ticket_fkey");
        });

        modelBuilder.Entity<HistoriqueStatut>(entity =>
        {
            entity.HasKey(e => e.IdHistorique).HasName("historique_statut_pkey");

            entity.ToTable("historique_statut");

            entity.HasIndex(e => e.IdTicket, "idx_historique_statut_ticket");

            entity.Property(e => e.IdHistorique).HasColumnName("id_historique");
            entity.Property(e => e.DateHeure)
                .HasDefaultValueSql("now()")
                .HasColumnName("date_heure");
            entity.Property(e => e.IdAuteur).HasColumnName("id_auteur");
            entity.Property(e => e.IdStatutPrecedent).HasColumnName("id_statut_precedent");
            entity.Property(e => e.IdStatutSuivant).HasColumnName("id_statut_suivant");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");

            entity.HasOne(d => d.IdAuteurNavigation).WithMany(p => p.HistoriqueStatuts)
                .HasForeignKey(d => d.IdAuteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("historique_statut_id_auteur_fkey");

            entity.HasOne(d => d.IdStatutPrecedentNavigation).WithMany(p => p.HistoriqueStatutIdStatutPrecedentNavigations)
                .HasForeignKey(d => d.IdStatutPrecedent)
                .HasConstraintName("historique_statut_id_statut_precedent_fkey");

            entity.HasOne(d => d.IdStatutSuivantNavigation).WithMany(p => p.HistoriqueStatutIdStatutSuivantNavigations)
                .HasForeignKey(d => d.IdStatutSuivant)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("historique_statut_id_statut_suivant_fkey");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.HistoriqueStatuts)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("historique_statut_id_ticket_fkey");
        });

        modelBuilder.Entity<JourFerie>(entity =>
        {
            entity.HasKey(e => e.IdJourFerie).HasName("jour_ferie_pkey");

            entity.ToTable("jour_ferie");

            entity.HasIndex(e => e.Date, "jour_ferie_date_key").IsUnique();

            entity.Property(e => e.IdJourFerie).HasColumnName("id_jour_ferie");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Libelle)
                .HasMaxLength(150)
                .HasColumnName("libelle");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.IdNotification).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.HasIndex(e => new { e.IdDestinataire, e.EstLue }, "idx_notification_destinataire_non_lues").HasFilter("(est_lue = false)");

            entity.HasIndex(e => e.IdTicket, "idx_notification_ticket");

            entity.Property(e => e.IdNotification).HasColumnName("id_notification");
            entity.Property(e => e.DateCreation)
                .HasDefaultValueSql("now()")
                .HasColumnName("date_creation");
            entity.Property(e => e.DateLecture).HasColumnName("date_lecture");
            entity.Property(e => e.EstLue).HasColumnName("est_lue");
            entity.Property(e => e.IdDestinataire).HasColumnName("id_destinataire");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.IdTypeEvenement).HasColumnName("id_type_evenement");

            entity.HasOne(d => d.IdDestinataireNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdDestinataire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notification_id_destinataire_fkey");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notification_id_ticket_fkey");

            entity.HasOne(d => d.IdTypeEvenementNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdTypeEvenement)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notification_id_type_evenement_fkey");
        });

        modelBuilder.Entity<PieceJointe>(entity =>
        {
            entity.HasKey(e => e.IdPieceJointe).HasName("piece_jointe_pkey");

            entity.ToTable("piece_jointe");

            entity.HasIndex(e => e.IdEmailSource, "idx_piece_jointe_email");

            entity.Property(e => e.IdPieceJointe).HasColumnName("id_piece_jointe");
            entity.Property(e => e.Chemin)
                .HasMaxLength(500)
                .HasColumnName("chemin");
            entity.Property(e => e.IdEmailSource).HasColumnName("id_email_source");
            entity.Property(e => e.NomFichier)
                .HasMaxLength(255)
                .HasColumnName("nom_fichier");
            entity.Property(e => e.TailleOctets).HasColumnName("taille_octets");
            entity.Property(e => e.TypeMime)
                .HasMaxLength(150)
                .HasColumnName("type_mime");

            entity.HasOne(d => d.IdEmailSourceNavigation).WithMany(p => p.PieceJointes)
                .HasForeignKey(d => d.IdEmailSource)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("piece_jointe_id_email_source_fkey");
        });

        modelBuilder.Entity<Reassignation>(entity =>
        {
            entity.HasKey(e => e.IdReassignation).HasName("reassignation_pkey");

            entity.ToTable("reassignation");

            entity.HasIndex(e => e.IdTicket, "idx_reassignation_ticket");

            entity.Property(e => e.IdReassignation).HasColumnName("id_reassignation");
            entity.Property(e => e.DateReassignation)
                .HasDefaultValueSql("now()")
                .HasColumnName("date_reassignation");
            entity.Property(e => e.IdAncienAssigne).HasColumnName("id_ancien_assigne");
            entity.Property(e => e.IdAuteur).HasColumnName("id_auteur");
            entity.Property(e => e.IdNouvelAssigne).HasColumnName("id_nouvel_assigne");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.Motif).HasColumnName("motif");

            entity.HasOne(d => d.IdAncienAssigneNavigation).WithMany(p => p.ReassignationIdAncienAssigneNavigations)
                .HasForeignKey(d => d.IdAncienAssigne)
                .HasConstraintName("reassignation_id_ancien_assigne_fkey");

            entity.HasOne(d => d.IdAuteurNavigation).WithMany(p => p.ReassignationIdAuteurNavigations)
                .HasForeignKey(d => d.IdAuteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reassignation_id_auteur_fkey");

            entity.HasOne(d => d.IdNouvelAssigneNavigation).WithMany(p => p.ReassignationIdNouvelAssigneNavigations)
                .HasForeignKey(d => d.IdNouvelAssigne)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reassignation_id_nouvel_assigne_fkey");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.Reassignations)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reassignation_id_ticket_fkey");
        });

        modelBuilder.Entity<RegleCriticite>(entity =>
        {
            entity.HasKey(e => e.IdRegleCriticite).HasName("regle_criticite_pkey");

            entity.ToTable("regle_criticite");

            entity.HasIndex(e => new { e.IdCs, e.IdTypeDemande }, "regle_criticite_id_cs_id_type_demande_key").IsUnique();

            entity.Property(e => e.IdRegleCriticite).HasColumnName("id_regle_criticite");
            entity.Property(e => e.IdCriticite).HasColumnName("id_criticite");
            entity.Property(e => e.IdCs).HasColumnName("id_cs");
            entity.Property(e => e.IdTypeDemande).HasColumnName("id_type_demande");

            entity.HasOne(d => d.IdCriticiteNavigation).WithMany(p => p.RegleCriticites)
                .HasForeignKey(d => d.IdCriticite)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("regle_criticite_id_criticite_fkey");

            entity.HasOne(d => d.IdCsNavigation).WithMany(p => p.RegleCriticites)
                .HasForeignKey(d => d.IdCs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("regle_criticite_id_cs_fkey");

            entity.HasOne(d => d.IdTypeDemandeNavigation).WithMany(p => p.RegleCriticites)
                .HasForeignKey(d => d.IdTypeDemande)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("regle_criticite_id_type_demande_fkey");
        });

        modelBuilder.Entity<Rejet>(entity =>
        {
            entity.HasKey(e => e.IdRejet).HasName("rejet_pkey");

            entity.ToTable("rejet");

            entity.HasIndex(e => e.IdTicket, "idx_rejet_ticket");

            entity.HasIndex(e => e.IdTicket, "uq_rejet_valide_par_ticket")
                .IsUnique()
                .HasFilter("(decision = true)");

            entity.Property(e => e.IdRejet).HasColumnName("id_rejet");
            entity.Property(e => e.DateDecision).HasColumnName("date_decision");
            entity.Property(e => e.DateProposition)
                .HasDefaultValueSql("now()")
                .HasColumnName("date_proposition");
            entity.Property(e => e.Decision).HasColumnName("decision");
            entity.Property(e => e.IdAuteur).HasColumnName("id_auteur");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.IdValidateur).HasColumnName("id_validateur");
            entity.Property(e => e.Justificatif).HasColumnName("justificatif");

            entity.HasOne(d => d.IdAuteurNavigation).WithMany(p => p.RejetIdAuteurNavigations)
                .HasForeignKey(d => d.IdAuteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rejet_id_auteur_fkey");

            entity.HasOne(d => d.IdTicketNavigation).WithOne(p => p.Rejet)
                .HasForeignKey<Rejet>(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rejet_id_ticket_fkey");

            entity.HasOne(d => d.IdValidateurNavigation).WithMany(p => p.RejetIdValidateurNavigations)
                .HasForeignKey(d => d.IdValidateur)
                .HasConstraintName("rejet_id_validateur_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("role_pkey");

            entity.ToTable("role");

            entity.HasIndex(e => e.Libelle, "role_libelle_key").IsUnique();

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.Libelle)
                .HasMaxLength(50)
                .HasColumnName("libelle");
        });

        modelBuilder.Entity<ServiceAccountToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_service_account_tokens");

            entity.ToTable("service_account_tokens");

            entity.HasIndex(e => e.Email, "idx_service_account_tokens_email");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessTokenExpiresAt).HasColumnName("access_token_expires_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.EncryptedAccessToken).HasColumnName("encrypted_access_token");
            entity.Property(e => e.EncryptedRefreshToken).HasColumnName("encrypted_refresh_token");
            entity.Property(e => e.Scopes).HasColumnName("scopes");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Sla>(entity =>
        {
            entity.HasKey(e => e.IdSla).HasName("sla_pkey");

            entity.ToTable("sla");

            entity.HasIndex(e => new { e.IdCs, e.IdTypeDemande }, "sla_id_cs_id_type_demande_key").IsUnique();

            entity.Property(e => e.IdSla).HasColumnName("id_sla");
            entity.Property(e => e.Duree)
                .HasPrecision(6, 2)
                .HasColumnName("duree");
            entity.Property(e => e.IdCs).HasColumnName("id_cs");
            entity.Property(e => e.IdTypeDemande).HasColumnName("id_type_demande");

            entity.HasOne(d => d.IdCsNavigation).WithMany(p => p.Slas)
                .HasForeignKey(d => d.IdCs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sla_id_cs_fkey");

            entity.HasOne(d => d.IdTypeDemandeNavigation).WithMany(p => p.Slas)
                .HasForeignKey(d => d.IdTypeDemande)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sla_id_type_demande_fkey");
        });

        modelBuilder.Entity<Statut>(entity =>
        {
            entity.HasKey(e => e.IdStatut).HasName("statut_pkey");

            entity.ToTable("statut");

            entity.HasIndex(e => e.Libelle, "statut_libelle_key").IsUnique();

            entity.Property(e => e.IdStatut).HasColumnName("id_statut");
            entity.Property(e => e.Libelle)
                .HasMaxLength(50)
                .HasColumnName("libelle");

            entity.HasMany(d => d.IdStatutDestinations).WithMany(p => p.IdStatutOrigines)
                .UsingEntity<Dictionary<string, object>>(
                    "TransitionAutorisee",
                    r => r.HasOne<Statut>().WithMany()
                        .HasForeignKey("IdStatutDestination")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("transition_autorisee_id_statut_destination_fkey"),
                    l => l.HasOne<Statut>().WithMany()
                        .HasForeignKey("IdStatutOrigine")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("transition_autorisee_id_statut_origine_fkey"),
                    j =>
                    {
                        j.HasKey("IdStatutOrigine", "IdStatutDestination").HasName("transition_autorisee_pkey");
                        j.ToTable("transition_autorisee");
                        j.IndexerProperty<int>("IdStatutOrigine").HasColumnName("id_statut_origine");
                        j.IndexerProperty<int>("IdStatutDestination").HasColumnName("id_statut_destination");
                    });

            entity.HasMany(d => d.IdStatutOrigines).WithMany(p => p.IdStatutDestinations)
                .UsingEntity<Dictionary<string, object>>(
                    "TransitionAutorisee",
                    r => r.HasOne<Statut>().WithMany()
                        .HasForeignKey("IdStatutOrigine")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("transition_autorisee_id_statut_origine_fkey"),
                    l => l.HasOne<Statut>().WithMany()
                        .HasForeignKey("IdStatutDestination")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("transition_autorisee_id_statut_destination_fkey"),
                    j =>
                    {
                        j.HasKey("IdStatutOrigine", "IdStatutDestination").HasName("transition_autorisee_pkey");
                        j.ToTable("transition_autorisee");
                        j.IndexerProperty<int>("IdStatutOrigine").HasColumnName("id_statut_origine");
                        j.IndexerProperty<int>("IdStatutDestination").HasColumnName("id_statut_destination");
                    });
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.IdTicket).HasName("ticket_pkey");

            entity.ToTable("ticket");

            entity.HasIndex(e => e.IdApplication, "idx_ticket_application");

            entity.HasIndex(e => e.IdCriticite, "idx_ticket_criticite");

            entity.HasIndex(e => e.DateCreation, "idx_ticket_date_creation");

            entity.HasIndex(e => e.IdStatut, "idx_ticket_statut");

            entity.HasIndex(e => e.IdTechnicienAssigne, "idx_ticket_technicien_assigne");

            entity.HasIndex(e => e.NumeroTicket, "ticket_numero_ticket_key").IsUnique();

            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.DateCloture).HasColumnName("date_cloture");
            entity.Property(e => e.DateCreation)
                .HasDefaultValueSql("now()")
                .HasColumnName("date_creation");
            entity.Property(e => e.DemandeurDirection)
                .HasMaxLength(150)
                .HasColumnName("demandeur_direction");
            entity.Property(e => e.DemandeurEmail)
                .HasMaxLength(255)
                .HasColumnName("demandeur_email");
            entity.Property(e => e.DureeSla)
                .HasPrecision(6, 2)
                .HasColumnName("duree_sla");
            entity.Property(e => e.IdApplication).HasColumnName("id_application");
            entity.Property(e => e.IdCriticite).HasColumnName("id_criticite");
            entity.Property(e => e.IdStatut).HasColumnName("id_statut");
            entity.Property(e => e.IdTechnicienAssigne).HasColumnName("id_technicien_assigne");
            entity.Property(e => e.IdTypeDemande).HasColumnName("id_type_demande");
            entity.Property(e => e.NumeroTicket)
                .HasMaxLength(30)
                .HasColumnName("numero_ticket");

            entity.HasOne(d => d.IdApplicationNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdApplication)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_id_application_fkey");

            entity.HasOne(d => d.IdCriticiteNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdCriticite)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_id_criticite_fkey");

            entity.HasOne(d => d.IdStatutNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdStatut)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_id_statut_fkey");

            entity.HasOne(d => d.IdTechnicienAssigneNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdTechnicienAssigne)
                .HasConstraintName("ticket_id_technicien_assigne_fkey");

            entity.HasOne(d => d.IdTypeDemandeNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdTypeDemande)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_id_type_demande_fkey");
        });

        modelBuilder.Entity<TypeDemande>(entity =>
        {
            entity.HasKey(e => e.IdTypeDemande).HasName("type_demande_pkey");

            entity.ToTable("type_demande");

            entity.HasIndex(e => e.Libelle, "type_demande_libelle_key").IsUnique();

            entity.Property(e => e.IdTypeDemande).HasColumnName("id_type_demande");
            entity.Property(e => e.Libelle)
                .HasMaxLength(50)
                .HasColumnName("libelle");
        });

        modelBuilder.Entity<TypeEvenementNotification>(entity =>
        {
            entity.HasKey(e => e.IdTypeEvenement).HasName("type_evenement_notification_pkey");

            entity.ToTable("type_evenement_notification");

            entity.HasIndex(e => e.Libelle, "type_evenement_notification_libelle_key").IsUnique();

            entity.Property(e => e.IdTypeEvenement).HasColumnName("id_type_evenement");
            entity.Property(e => e.Libelle)
                .HasMaxLength(150)
                .HasColumnName("libelle");
        });

        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.IdUtilisateur).HasName("utilisateur_pkey");

            entity.ToTable("utilisateurs");

            entity.HasIndex(e => e.Actif, "idx_utilisateur_actif");

            entity.HasIndex(e => e.IdRole, "idx_utilisateur_role");

            entity.HasIndex(e => e.Email, "utilisateur_email_key").IsUnique();

            entity.HasIndex(e => e.IdentifiantAd, "utilisateur_identifiant_ad_key").IsUnique();

            entity.Property(e => e.IdUtilisateur)
                .HasDefaultValueSql("nextval('utilisateur_id_utilisateur_seq'::regclass)")
                .HasColumnName("id_utilisateur");
            entity.Property(e => e.Actif)
                .HasDefaultValue(true)
                .HasColumnName("actif");
            entity.Property(e => e.DateDesactivation).HasColumnName("date_desactivation");
            entity.Property(e => e.DateSynchronisation)
                .HasDefaultValueSql("now()")
                .HasColumnName("date_synchronisation");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.IdentifiantAd)
                .HasMaxLength(100)
                .HasColumnName("identifiant_ad");
            entity.Property(e => e.Nom)
                .HasMaxLength(100)
                .HasColumnName("nom");
            entity.Property(e => e.Prenom)
                .HasMaxLength(100)
                .HasColumnName("prenom");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Utilisateurs)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("utilisateur_id_role_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
