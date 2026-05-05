using System.Text;
using FCG.API.GraphQL;
using FCG.API.Middlewares;
using FCG.API.Security;
using FCG.Application;
using FCG.Application.Abstractions;
using FCG.Domain.Entities;
using FCG.Infrastructure;
using FCG.Infrastructure.Persistence;
using FCG.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// ---------------- Serilog (bootstrap) ----------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "FCG.API")
        .WriteTo.Console()
        .WriteTo.File("logs/fcg-.log", rollingInterval: RollingInterval.Day));

    // ---------------- DI: camadas ----------------
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // ---------------- Auth (JWT) ----------------
    var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
    var jwtSettings = jwtSection.Get<JwtSettings>()
        ?? throw new InvalidOperationException("Configuração JWT ausente.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

    builder.Services.AddAuthorization();

    // ---------------- MVC + Swagger ----------------
    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "FIAP Cloud Games (FCG) API",
            Version = "v1",
            Description = "API REST de cadastro de usuários e biblioteca de jogos — Tech Challenge Fase 1."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Cole o JWT no formato: Bearer {seu_token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
    });

    // ---------------- GraphQL (HotChocolate) ----------------
    builder.Services
        .AddGraphQLServer()
        .AddQueryType(d => d.Name("Query"))
        .AddTypeExtension<GameQueries>()
        .AddProjections()
        .AddFiltering()
        .AddSorting();

    // ---------------- App ----------------
    var app = builder.Build();

    // Aplicar migrations automaticamente em ambiente Development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        await SeedAdminAsync(scope.ServiceProvider);
    }

    app.UseSerilogRequestLogging();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGraphQL("/graphql");

    app.MapGet("/", () => Results.Redirect("/swagger"));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha fatal ao iniciar a aplicação");
}
finally
{
    Log.CloseAndFlush();
}

// Seed do administrador inicial para facilitar a demonstração
static async Task SeedAdminAsync(IServiceProvider sp)
{
    var db = sp.GetRequiredService<ApplicationDbContext>();
    if (!db.Users.Any())
    {
        var hasher = sp.GetRequiredService<IPasswordHasher>();
        var admin = User.Create(
            "Administrador FCG",
            FCG.Domain.ValueObjects.Email.Create("admin@fcg.com"),
            hasher.Hash("Admin@123"),
            FCG.Domain.Enums.UserRole.Admin);
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}

// Exposta para WebApplicationFactory em testes de integração.
public partial class Program { }
