using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Data.Repositories;
using SIGRA.Domain.Options;
using SIGRA.Hubs;
using SIGRA.Middleware;
using SIGRA.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"], o => o.MapEnum<OAuthProvider>("oauth_provider")));

builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

builder.Services.AddSignalR();

builder.Services.AddScoped<IServiceAccountTokenRepository, ServiceAccountTokenRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IStatutRepository, StatutRepository>();
builder.Services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();
builder.Services.AddScoped<ITicketService, TicketService>();
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
builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
builder.Services.AddScoped<IEmailsSourceRepository, EmailsSourceRepository>();
builder.Services.AddScoped<IPiecesJointeRepository, PiecesJointeRepository>();
builder.Services.AddScoped<IStorageService, FileSystemStorageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<ImapMailService>();
builder.Services.AddSingleton<ImapSyncService>();
builder.Services.AddSingleton<IImapIdentityProvider, GmailIdentityProvider>();
builder.Services.AddHostedService<ImapPollingService>();


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
    options.DefaultAuthenticateScheme = "Mock";
    options.DefaultChallengeScheme = "Mock";
})
.AddScheme<MockAuthenticationOptions, MockAuthenticationHandler>("Mock", options =>
{
    options.HeaderName = "X-Mock-User";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ADAuthorizedUser", policy =>
    {
        policy.AddAuthenticationSchemes("Mock");
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

app.MapHub<NotificationHub>("/hubs/notifications");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("sigra-client");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorizedUserMiddleware();
app.UseAuthorization();

app.MapControllers();

app.Run();
