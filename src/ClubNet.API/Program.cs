using System.Text.Json.Serialization;
using ClubNet.API.Data;
using ClubNet.API.Security;
using ClubNet.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(options => options.DefaultForbidScheme = ApiStatusCodeAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiStatusCodeAuthenticationHandler>(
        ApiStatusCodeAuthenticationHandler.SchemeName,
        _ => { });

// Controllers & JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configurações JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<IJwtService, JwtService>();

// Contexto do Usuário
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Configuração do Banco de Dados (Suporte a SQLite e MySQL)
var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var connectionString = builder.Configuration.GetConnectionString(
    provider.Equals("MySql", StringComparison.OrdinalIgnoreCase) ? "MySqlConnection" : "DefaultConnection"
);

if (!provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
{
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "clubnet.db");
    connectionString = $"Data Source={dbPath}";
}

builder.Services.AddDbContext<ClubNetDbContext>(options =>
{
    if (provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 31));
        options.UseMySql(connectionString!, serverVersion);
    }
    else
    {
        options.UseSqlite(connectionString!);
    }
});

// CORS para permitir conexões do Site Web e do App Mobile
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed do banco de dados na inicialização
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClubNetDbContext>();
    await DbInitializer.InitializeAsync(db);
}

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

// Middleware de Autenticação JWT personalizado
app.UseMiddleware<JwtAuthMiddleware>();

app.UseRouting();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    Nome = "ClubNet API - Sistema de Gestão de Clubes",
    Status = "Operacional",
    Versao = "1.0.0",
    DataHora = DateTime.UtcNow
}));

app.Run();
