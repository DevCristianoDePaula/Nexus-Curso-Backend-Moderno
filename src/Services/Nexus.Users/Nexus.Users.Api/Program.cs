using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Nexus.Shared.Observability;
using Nexus.Users.Application;
using Nexus.Users.Infrastructure;
using Scalar.AspNetCore;

// Cria o builder da aplicacao Web API
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONFIGURACAO DE SERVICOS (Dependency Injection)
// ============================================================

// Registra as dependecias da Infraestrutura (DbContext, repositorios, etc.)
builder.Services.AddUsersInfrastructure(builder.Configuration);
// Adiciona observabilidade (OpenTelemetry, logs, metricas)
builder.Services.AddNexusObservability("Nexus.Users.Api");
// Habilita a documentacao OpenAPI / Swagger
builder.Services.AddOpenApi();

// ============================================================
// AUTENTICACAO JWT
// ============================================================
// Configura a autenticacao via tokens JWT (JSON Web Token).
// O servidor valida o token em cada requisicao protegida.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Define os parametros de validacao do token JWT
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,                  // Verifica se o emissor (Issuer) e confiavel
            ValidateAudience = true,                // Verifica se a audiencia (Audience) esta correta
            ValidateLifetime = true,                // Verifica se o token nao expirou
            ValidateIssuerSigningKey = true,        // Verifica se a chave de assinatura e valida
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Nexus",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Nexus",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "Nexus-Temporary-Dev-Key-Minimum-32-Characters!"))
        };
    });

// Habilita a autorizacao baseada em politicas/claims (usado com [Authorize] ou RequireAuthorization())
builder.Services.AddAuthorization();
// Endpoint de health check para monitoramento
builder.Services.AddHealthChecks();

// ============================================================
// CONSTRUCAO DA APLICACAO E MIDDLEWARE PIPELINE
// ============================================================

var app = builder.Build();

// Garante que o banco de dados exista em ambiente de desenvolvimento
// (nao ha migrations formais neste projeto ainda)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<Nexus.Users.Infrastructure.NexusIdentityDbContext>();
    dbContext.Database.EnsureCreated();

    // Seed das roles (Customer, Seller, Admin) usadas pelo AuthService.RegisterAsync
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var roleName in Enum.GetNames<Nexus.Users.Domain.UserType>())
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}

// Security Headers (OWASP)
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

    // A UI do Scalar (/scalar) precisa de script inline e recursos externos (CDN);
    // um CSP restritivo ("default-src 'self'") bloqueia a inicializacao e deixa a pagina em branco.
    if (!ctx.Request.Path.StartsWithSegments("/scalar"))
    {
        ctx.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    }

    await next();
});

app.MapOpenApi();
app.MapScalarApiReference();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");

var auth = app.MapGroup("/api/auth");

auth.MapPost("/register", async (RegisterRequest request, AuthService authService) =>
{
    var result = await authService.RegisterAsync(request);
    return result.Succeeded
        ? Results.Created("/api/auth/login", new { message = "User registered" })
        : Results.BadRequest(new { errors = result.Errors });
});

auth.MapPost("/login", async (LoginRequest request, AuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    return result.Succeeded
        ? Results.Ok(new { token = result.Token, refreshToken = result.RefreshToken })
        : Results.Unauthorized();
});

auth.MapPost("/refresh", async (RefreshTokenRequest request, AuthService authService) =>
{
    var result = await authService.RefreshTokenAsync(request.RefreshToken);
    return result.Succeeded
        ? Results.Ok(new { token = result.Token, refreshToken = result.RefreshToken })
        : Results.Unauthorized();
});

auth.MapPost("/logout", async (HttpContext ctx, AuthService authService) =>
{
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userId is null)
        return Results.Unauthorized();
    var result = await authService.LogoutAsync(userId);
    return result.Succeeded
        ? Results.Ok(new { message = "Logged out" })
        : Results.BadRequest();
}).RequireAuthorization();

auth.MapGet("/me", (HttpContext ctx) =>
{
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var email = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var name = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    return Results.Ok(new { userId, email, name });
}).RequireAuthorization();

app.Run();
