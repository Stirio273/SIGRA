using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Data.Repositories;
using SIGRA.Middleware;
using SIGRA.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"], o => o.MapEnum<OAuthProvider>("oauth_provider")));


builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IServiceAccountTokenRepository, ServiceAccountTokenRepository>();
builder.Services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();
builder.Services.AddSingleton<ImapMailService>();
builder.Services.AddSingleton<ImapSyncService>();
builder.Services.AddSingleton<IImapIdentityProvider, GmailIdentityProvider>();
builder.Services.AddHostedService<ImapPollingService>();


var allowSpecificOrigins = "sigra-client";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddDataProtection();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseCors(allowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorizedUserMiddleware();
app.UseAuthorization();

app.MapControllers();

app.Run();
