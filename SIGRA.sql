-- Extension réservée pour l'usage futur de l'assistant IA (RAG post-MVP)
CREATE EXTENSION IF NOT EXISTS vector;

-- Extension pour la recherche plein texte non-accentuée (utile en français)
CREATE EXTENSION IF NOT EXISTS unaccent;

-- ============================================================================
-- BLOC 1 : UTILISATEURS & RÔLES
-- ============================================================================

CREATE TABLE role (
    id_role         SERIAL PRIMARY KEY,
    libelle         VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE utilisateur (
    id_utilisateur      SERIAL PRIMARY KEY,
    identifiant_ad      VARCHAR(100) NOT NULL UNIQUE,
    nom                 VARCHAR(100) NOT NULL,
    prenom              VARCHAR(100) NOT NULL,
    email               VARCHAR(255) NOT NULL UNIQUE,
    actif               BOOLEAN NOT NULL DEFAULT TRUE,
    date_desactivation  TIMESTAMPTZ,
    date_synchronisation TIMESTAMPTZ NOT NULL DEFAULT now(),
    id_role             INTEGER NOT NULL REFERENCES role(id_role),
    CONSTRAINT chk_date_desactivation_coherente
        CHECK (
            (actif = TRUE AND date_desactivation IS NULL)
            OR (actif = FALSE)
        )
);

CREATE INDEX idx_utilisateur_role ON utilisateur(id_role);
CREATE INDEX idx_utilisateur_actif ON utilisateur(actif);

-- ============================================================================
-- BLOC 2 : APPLICATION, CLASSE DE SERVICE & CRITICITÉ
-- ============================================================================

CREATE TABLE classe_de_service (
    id_cs           SERIAL PRIMARY KEY,
    code            VARCHAR(20) NOT NULL UNIQUE,
    libelle         VARCHAR(100)
);

CREATE TABLE application (
    id_application  SERIAL PRIMARY KEY,
    libelle         VARCHAR(150) NOT NULL,
    actif           BOOLEAN NOT NULL DEFAULT TRUE,
    id_cs           INTEGER NOT NULL REFERENCES classe_de_service(id_cs)
);

CREATE INDEX idx_application_cs ON application(id_cs);
CREATE INDEX idx_application_actif ON application(actif);

CREATE TABLE type_demande (
    id_type_demande SERIAL PRIMARY KEY,
    libelle         VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE criticite (
    id_criticite    SERIAL PRIMARY KEY,
    libelle         VARCHAR(50) NOT NULL UNIQUE,
    ordre           INTEGER NOT NULL UNIQUE
);

CREATE TABLE regle_criticite (
    id_regle_criticite  SERIAL PRIMARY KEY,
    id_cs               INTEGER NOT NULL REFERENCES classe_de_service(id_cs),
    id_type_demande     INTEGER NOT NULL REFERENCES type_demande(id_type_demande),
    id_criticite        INTEGER NOT NULL REFERENCES criticite(id_criticite),
    UNIQUE (id_cs, id_type_demande)
);

-- ============================================================================
-- BLOC 3 : TICKET & CYCLE DE VIE
-- ============================================================================

CREATE TABLE statut (
    id_statut       SERIAL PRIMARY KEY,
    libelle         VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE transition_autorisee (
    id_statut_origine       INTEGER NOT NULL REFERENCES statut(id_statut),
    id_statut_destination   INTEGER NOT NULL REFERENCES statut(id_statut),
    PRIMARY KEY (id_statut_origine, id_statut_destination),
    CONSTRAINT chk_transition_differente
        CHECK (id_statut_origine <> id_statut_destination)
);

CREATE TABLE ticket (
    id_ticket               SERIAL PRIMARY KEY,
    numero_ticket           VARCHAR(30) NOT NULL UNIQUE,
    date_creation            TIMESTAMPTZ NOT NULL DEFAULT now(),
    id_application           INTEGER NOT NULL REFERENCES application(id_application),
    id_type_demande          INTEGER NOT NULL REFERENCES type_demande(id_type_demande),
    id_criticite             INTEGER NOT NULL REFERENCES criticite(id_criticite),
    id_statut                INTEGER NOT NULL REFERENCES statut(id_statut),
    id_technicien_assigne    INTEGER REFERENCES utilisateur(id_utilisateur),
    demandeur_email          VARCHAR(255) NOT NULL,
    demandeur_direction      VARCHAR(150) NOT NULL,
    date_cloture             TIMESTAMPTZ,
    duree_sla                NUMERIC(6,2) NOT NULL,
    CONSTRAINT chk_date_cloture_coherente
        CHECK (date_cloture IS NULL OR date_cloture >= date_creation)
);

-- Index sur les colonnes les plus filtrées au tableau de bord
CREATE INDEX idx_ticket_statut ON ticket(id_statut);
CREATE INDEX idx_ticket_technicien_assigne ON ticket(id_technicien_assigne);
CREATE INDEX idx_ticket_application ON ticket(id_application);
CREATE INDEX idx_ticket_criticite ON ticket(id_criticite);
CREATE INDEX idx_ticket_date_creation ON ticket(date_creation);

CREATE TABLE historique_statut (
    id_historique           SERIAL PRIMARY KEY,
    id_ticket               INTEGER NOT NULL REFERENCES ticket(id_ticket),
    id_statut_precedent     INTEGER REFERENCES statut(id_statut),
    id_statut_suivant       INTEGER NOT NULL REFERENCES statut(id_statut),
    id_auteur               INTEGER NOT NULL REFERENCES utilisateur(id_utilisateur),
    date_heure              TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_historique_statut_ticket ON historique_statut(id_ticket);

-- ============================================================================
-- BLOC 4 : EMAIL SOURCE (métadonnées d'ingestion)
-- ============================================================================

CREATE TABLE email_source (
    id_email_source         SERIAL PRIMARY KEY,
    id_ticket               INTEGER NOT NULL REFERENCES ticket(id_ticket),
    message_id_graph        VARCHAR(255) NOT NULL UNIQUE,
    conversation_id_graph   VARCHAR(255) NOT NULL,
    expediteur              VARCHAR(255) NOT NULL,
    objet                   VARCHAR(500),
    corps_email             TEXT,
    date_reception          TIMESTAMPTZ NOT NULL,
    est_email_initial       BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_email_source_ticket ON email_source(id_ticket);
CREATE INDEX idx_email_source_conversation ON email_source(conversation_id_graph);

-- Garantit qu'un seul email par ticket est marqué "initial"
CREATE UNIQUE INDEX uq_email_source_initial
    ON email_source(id_ticket)
    WHERE est_email_initial = TRUE;

-- ============================================================================
-- BLOC 5 : COMMENTAIRES & PIÈCES JOINTES
-- ============================================================================

CREATE TABLE piece_jointe (
    id_piece_jointe     SERIAL PRIMARY KEY,
    id_email_source     INTEGER NOT NULL REFERENCES email_source(id_email_source),
    nom_fichier         VARCHAR(255) NOT NULL,
    chemin              VARCHAR(500) NOT NULL,
    taille_octets       BIGINT,
    type_mime           VARCHAR(150)
);

CREATE INDEX idx_piece_jointe_email ON piece_jointe(id_email_source);

CREATE TABLE commentaire (
    id_commentaire      SERIAL PRIMARY KEY,
    id_ticket           INTEGER NOT NULL REFERENCES ticket(id_ticket),
    id_auteur           INTEGER NOT NULL REFERENCES utilisateur(id_utilisateur),
    contenu             TEXT NOT NULL,
    date_creation       TIMESTAMPTZ NOT NULL DEFAULT now(),
    est_note_resolution BOOLEAN NOT NULL DEFAULT FALSE,
    -- Colonne générée pour la recherche plein texte (français)
    contenu_tsv         TSVECTOR GENERATED ALWAYS AS (to_tsvector('french', contenu)) STORED
);

CREATE INDEX idx_commentaire_ticket ON commentaire(id_ticket);
CREATE INDEX idx_commentaire_note_resolution ON commentaire(est_note_resolution) WHERE est_note_resolution = TRUE;
CREATE INDEX idx_commentaire_contenu_tsv ON commentaire USING GIN(contenu_tsv);

-- ============================================================================
-- BLOC 6 : REJET, RÉASSIGNATION & ESCALADE
-- ============================================================================

CREATE TABLE rejet (
    id_rejet                    SERIAL PRIMARY KEY,
    id_ticket                   INTEGER NOT NULL REFERENCES ticket(id_ticket),
    id_auteur                   INTEGER NOT NULL REFERENCES utilisateur(id_utilisateur),
    justificatif                TEXT NOT NULL,
    date_proposition            TIMESTAMPTZ NOT NULL DEFAULT now(),
    id_validateur                INTEGER REFERENCES utilisateur(id_utilisateur),
    decision                     BOOLEAN,
    date_decision                TIMESTAMPTZ,
    CONSTRAINT chk_decision_coherente
        CHECK (
            (decision IS NULL AND id_validateur IS NULL AND date_decision IS NULL)
            OR (decision IS NOT NULL AND id_validateur IS NOT NULL AND date_decision IS NOT NULL)
        )
);

CREATE INDEX idx_rejet_ticket ON rejet(id_ticket);

-- Un seul rejet VALIDÉ (decision = true) par ticket, conformément à la règle métier
CREATE UNIQUE INDEX uq_rejet_valide_par_ticket
    ON rejet(id_ticket)
    WHERE decision = TRUE;

CREATE TABLE entite_externe (
    id_entite_externe   SERIAL PRIMARY KEY,
    nom                 VARCHAR(150) NOT NULL,
    actif               BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE escalade (
    id_escalade             SERIAL PRIMARY KEY,
    id_ticket               INTEGER NOT NULL REFERENCES ticket(id_ticket),
    id_entite_externe       INTEGER NOT NULL REFERENCES entite_externe(id_entite_externe),
    id_auteur               INTEGER NOT NULL REFERENCES utilisateur(id_utilisateur),
    date_escalade           TIMESTAMPTZ NOT NULL DEFAULT now(),
    explication             TEXT NOT NULL,
    est_definitif           BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_escalade_ticket ON escalade(id_ticket);
CREATE INDEX idx_escalade_entite ON escalade(id_entite_externe);

CREATE TABLE reassignation (
    id_reassignation    SERIAL PRIMARY KEY,
    id_ticket           INTEGER NOT NULL REFERENCES ticket(id_ticket),
    id_ancien_assigne   INTEGER REFERENCES utilisateur(id_utilisateur),
    id_nouvel_assigne   INTEGER NOT NULL REFERENCES utilisateur(id_utilisateur),
    motif               TEXT NOT NULL,
    id_auteur           INTEGER NOT NULL REFERENCES utilisateur(id_utilisateur),
    date_reassignation  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_reassignation_ticket ON reassignation(id_ticket);

-- ============================================================================
-- BLOC 7 : SLA & JOURS FÉRIÉS
-- ============================================================================

CREATE TABLE sla (
    id_sla          SERIAL PRIMARY KEY,
    id_cs           INTEGER NOT NULL REFERENCES classe_de_service(id_cs),
    id_type_demande INTEGER NOT NULL REFERENCES type_demande(id_type_demande),
    duree           NUMERIC(6,2) NOT NULL CHECK (duree > 0),
    UNIQUE (id_cs, id_type_demande)
);

CREATE TABLE jour_ferie (
    id_jour_ferie   SERIAL PRIMARY KEY,
    date            DATE NOT NULL UNIQUE,
    libelle         VARCHAR(150) NOT NULL
);

-- ============================================================================
-- BLOC 8 : NOTIFICATIONS
-- ============================================================================

CREATE TABLE type_evenement_notification (
    id_type_evenement   SERIAL PRIMARY KEY,
    libelle             VARCHAR(150) NOT NULL UNIQUE
);

CREATE TABLE notification (
    id_notification     SERIAL PRIMARY KEY,
    id_destinataire      INTEGER NOT NULL REFERENCES utilisateur(id_utilisateur),
    id_ticket            INTEGER NOT NULL REFERENCES ticket(id_ticket),
    id_type_evenement    INTEGER NOT NULL REFERENCES type_evenement_notification(id_type_evenement),
    date_creation         TIMESTAMPTZ NOT NULL DEFAULT now(),
    est_lue               BOOLEAN NOT NULL DEFAULT FALSE,
    date_lecture          TIMESTAMPTZ,
    CONSTRAINT chk_date_lecture_coherente
        CHECK (
            (est_lue = FALSE AND date_lecture IS NULL)
            OR (est_lue = TRUE AND date_lecture IS NOT NULL)
        )
);

-- Index composite pour la requête la plus fréquente : notifications non lues d'un utilisateur
CREATE INDEX idx_notification_destinataire_non_lues
    ON notification(id_destinataire, est_lue)
    WHERE est_lue = FALSE;

CREATE INDEX idx_notification_ticket ON notification(id_ticket);

-- ============================================================================
-- BLOC 9 : OAuth Token
-- ============================================================================

-- Création du type ENUM PostgreSQL pour les providers
CREATE TYPE oauth_provider AS ENUM (
    'Google',
    'Microsoft'
);

CREATE TABLE service_account_tokens
(
    id                      SERIAL          NOT NULL,

    email                   VARCHAR(256)    NOT NULL,
    provider                oauth_provider  NOT NULL,

    encrypted_access_token  TEXT            NOT NULL,
    encrypted_refresh_token TEXT                NULL,
    scopes                  TEXT                NULL,
    access_token_expires_at TIMESTAMPTZ         NULL,
    created_at              TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),
    updated_at              TIMESTAMPTZ     NOT NULL    DEFAULT NOW(),

    CONSTRAINT pk_service_account_tokens
        PRIMARY KEY (id),

    CONSTRAINT uq_service_account_tokens_email_provider
        UNIQUE (email, provider)
);

CREATE INDEX idx_service_account_tokens_email
    ON service_account_tokens (email);

CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER tr_service_account_tokens_updated_at
    BEFORE UPDATE ON service_account_tokens
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- DONNÉES DE RÉFÉRENCE INITIALES
-- ============================================================================

INSERT INTO role (libelle) VALUES ('Administrateur'), ('Technicien'), ('Manager');

INSERT INTO type_demande (libelle) VALUES ('Réclamation'), ('Demande');

INSERT INTO criticite (libelle, ordre) VALUES ('Critique', 1), ('Haute', 2), ('Normale', 3);

INSERT INTO statut (libelle) VALUES
    ('Nouveau'), ('En cours'), ('En attente'), ('Escaladé'),
    ('En attente de validation rejet'), ('Résolu'), ('Clôturé'), ('Rejeté');

-- Transitions autorisées
INSERT INTO transition_autorisee (id_statut_origine, id_statut_destination)
SELECT s1.id_statut, s2.id_statut
FROM statut s1, statut s2
WHERE (s1.libelle, s2.libelle) IN (
    ('Nouveau', 'En cours'),
    ('Nouveau', 'En attente de validation rejet'),
    ('En cours', 'En attente'),
    ('En cours', 'Escaladé'),
    ('En cours', 'En attente de validation rejet'),
    ('En cours', 'Résolu'),
    ('En attente', 'Escaladé'),
    ('Escaladé', 'Résolu'),
    ('En attente de validation rejet', 'Nouveau'),
    ('En attente de validation rejet', 'Rejeté'),
    ('Résolu', 'Clôturé')
);

INSERT INTO type_evenement_notification (libelle) VALUES
    ('Nouveau ticket créé'),
    ('Ticket en attente depuis plus de 48h'),
    ('Ticket escaladé depuis plus de 48h'),
    ('Ticket à 80% du délai SLA'),
    ('Proposition de rejet soumise'),
    ('Réassignation reçue');