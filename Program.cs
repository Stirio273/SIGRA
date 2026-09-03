using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Data.Repositories;
using SIGRA.Domain;
using SIGRA.Domain.Options;
using SIGRA.Domain.Rules;
using SIGRA.Hubs;
using SIGRA.Middleware;
using SIGRA.Services;
using SIGRA.Services.Handlers;
using SIGRA.Services.Providers;
using SIGRA.Views;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"], o => o.MapEnum<OAuthProvider>("oauth_provider")));

builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.Configure<SMTPOptions>(
    builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<BusinessHoursOptions>(
    builder.Configuration.GetSection("BusinessHours"));

builder.Services.AddSignalR();

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration["ConnectionStrings:HangfireConnection"]);
    }));

builder.Services.AddHangfireServer();


builder.Services.AddMemoryCache();
builder.Services.AddScoped<IHolidayProvider, HolidayProvider>();
builder.Services.AddScoped<IBusinessTimeCalculator, BusinessTimeCalculator>();
// builder.Services.AddScoped<ISlaPolicyProvider, SlaPolicyProvider>();
builder.Services.AddScoped<ITicketSlaService, TicketSlaService>();
builder.Services.AddScoped<IServiceAccountTokenRepository, ServiceAccountTokenRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IStatutRepository, StatutRepository>();
builder.Services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketExportService, TicketExportService>();
builder.Services.AddScoped<ICommentaireRepository, CommentaireRepository>();
builder.Services.AddScoped<ICommentaireService, CommentaireService>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IClassesServiceRepository, ClassesServiceRepository>();
builder.Services.AddScoped<IClassesServiceService, ClassesServiceService>();
builder.Services.AddScoped<ICriticiteRepository, CriticiteRepository>();
builder.Services.AddScoped<ICriticiteService, CriticiteService>();
builder.Services.AddScoped<IEntitesExterneRepository, EntitesExterneRepository>();
builder.Services.AddScoped<IEntitesExterneService, EntitesExterneService>();
builder.Services.AddScoped<IJoursFerieRepository, JoursFerieRepository>();
builder.Services.AddScoped<IJoursFerieService, JoursFerieService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
builder.Services.AddScoped<IEmailsSourceRepository, EmailsSourceRepository>();
builder.Services.AddScoped<IPiecesJointeRepository, PiecesJointeRepository>();
builder.Services.AddScoped<IStorageService, FileSystemStorageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IWeeklyReportBuilder, WeeklyReportBuilder>();
// builder.Services.AddScoped<ChartGenerator>();
// builder.Services.AddScoped<IPdfReportGenerator, PdfReportGenerator>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITicketAlertRule, WaitingTooLongRule>();
builder.Services.AddScoped<ITicketAlertRule, EscalatedTooLongRule>();
builder.Services.AddScoped<ITicketAlertRule, SlaThresholdRule>();
builder.Services.AddScoped<TicketAlertEvaluationService>();
builder.Services.AddScoped<IDomainEventHandler<TicketReopenedEvent>, RecordReopenHistoryHandler>();
builder.Services.AddScoped<IDomainEventHandler<TicketReopenedEvent>, NotifyTicketReopenedHandler>();
builder.Services.AddScoped<IDomainEventHandler<TicketReopenedEvent>, AlertOnRepeatedReopenHandler>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
// builder.Services.AddScoped<IAISupportAssistant, PlaceholderAISupportAssistant>();
builder.Services.AddScoped<IAIAssistantService, TicketAIAssistantService>();
builder.Services.AddScoped<ITicketContextProvider, TicketContextProvider>();
builder.Services.AddScoped<IKnowledgeRetriever, KeywordKnowledgeRetriever>();
builder.Services.AddScoped<ILlmClient, MockLlmClient>();
builder.Services.AddScoped<IAISupportOrchestrator, AiSupportOrchestrator>();
builder.Services.AddScoped<IPromptBuilder, TicketPromptBuilder>();
builder.Services.AddScoped<IAIResponseParser, JsonAiResponseParser>();
builder.Services.AddScoped<ISourceAttacher, KnowledgeSourceAttacher>();
builder.Services.AddScoped<CompositeKnowledgeRetriever>();
builder.Services.AddSingleton<IKnowledgeDocumentStore, InMemoryKnowledgeDocumentStore>();
builder.Services.AddSingleton<ITicketContentSanitizer, TicketContentSanitizer>();
builder.Services.AddSingleton<WeeklyReportViewModelMapper>();
builder.Services.AddSingleton<IWeeklyReportHtmlBuilder, WeeklyReportHtmlBuilder>();
builder.Services.AddSingleton<IPdfReportGenerator, PlaywrightPdfGenerationService>();
builder.Services.AddSingleton<ImapMailService>();
builder.Services.AddSingleton<ImapSyncService>();
builder.Services.AddSingleton<IImapIdentityProvider, GmailIdentityProvider>();
builder.Services.AddHostedService<ImapPollingService>();
builder.Services.AddHostedService<ReportBackgroundService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "sigra-client",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddDataProtection();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Fallback";
    options.DefaultChallengeScheme = "Fallback";
})
.AddScheme<FallbackAuthenticationOptions, FallbackAuthenticationHandler>("Fallback", options =>
{
})
.AddNegotiate()
.AddScheme<MockAuthenticationOptions, MockAuthenticationHandler>("Mock", options =>
{
    options.HeaderName = "X-Mock-User";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ADAuthorizedUser", policy =>
    {
        policy.AddAuthenticationSchemes("Fallback");
        policy.RequireAuthenticatedUser();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHangfireDashboard();

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Add or update your recurring job safely here
    recurringJobManager.AddOrUpdate<TicketAlertEvaluationService>(
        "ticket-alerts-evaluation",
        service => service.EvaluateAllRulesAsync(),
        "*/15 * * * *"
    );
}

// RecurringJob.AddOrUpdate<TicketAlertEvaluationService>(
//     "ticket-alerts-evaluation",
//     service => service.EvaluateAllRulesAsync(),
//     "*/15 * * * *");

app.MapHub<NotificationHub>("/hubs/notifications");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("sigra-client");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorizedUserMiddleware();
app.UseAuthorization();

app.MapGet("/whoami", (HttpContext ctx) => Results.Ok(new
{
    User = ctx.User.Identity?.Name,
    AuthType = ctx.User.Identity?.AuthenticationType
}));

app.MapControllers();

app.Run();
