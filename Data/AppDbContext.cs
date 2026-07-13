using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;

namespace SIGRA.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public virtual DbSet<ServiceAccountToken> ServiceAccountTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<OAuthProvider>();

        modelBuilder.Entity<ServiceAccountToken>(entity =>
        {
            entity.ToTable("service_account_tokens");

            entity.HasKey(e => e.Id)
                .HasName("pk_service_account_tokens");

            entity.HasIndex(e => e.Email)
                .HasDatabaseName("ix_service_account_tokens_email");

            entity.HasIndex(e => new { e.Email, e.Provider })
                .IsUnique()
                .HasDatabaseName("uq_service_account_tokens_email_provider");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(256);

            entity.Property(e => e.Provider)
                .HasColumnName("provider")
                .HasColumnType("oauth_provider");

            entity.Property(e => e.EncryptedAccessToken)
                .HasColumnName("encrypted_access_token");

            entity.Property(e => e.EncryptedRefreshToken)
                .HasColumnName("encrypted_refresh_token");

            entity.Property(e => e.Scopes)
                .HasColumnName("scopes");

            entity.Property(e => e.AccessTokenExpiresAt)
                .HasColumnName("access_token_expires_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("NOW()");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
