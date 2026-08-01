using System.Net;
using Microsoft.AspNetCore.RateLimiting;
using Nexus.Shared.Observability;
using Polly;
using Scalar.AspNetCore;

// O Gateway e o ponto de entrada unico da arquitetura de microservicos.
// Ele atua como proxy reverso, roteando requisicoes para os servicos internos
// (Catalog, Users, Cart, Orders, Payments, Coupons) e aplicando politicas
// transversais como CORS, Rate Limiting e Resiliciencia.

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONFIGURACAO DE SERVICOS (Dependency Injection)
// ============================================================

// Le as origens permitidas para CORS do appsettings.json (frontend em Vite)
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"];

// Configura CORS para permitir que o frontend acesse a API de origens diferentes
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins)          // Origens permitidas (ex: localhost:5173)
              .AllowAnyHeader()                   // Permite qualquer cabecalho HTTP
              .AllowAnyMethod()                   // Permite qualquer metodo (GET, POST, etc.)
              .AllowCredentials());               // Permite envio de cookies/autenticacao
});

// ============================================================
// PROXY REVERSO COM YARP
// ============================================================
// O YARP (Yet Another Reverse Proxy) roteia requisicoes para os microservicos
// com base nas regras definidas na secao "ReverseProxy" do appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ============================================================
// RATE LIMITING (Limitacao de Taxa)
// ============================================================
// Protege os servicos contra uso excessivo, limitando a 100 requisicoes
// por minuto por cliente (janela fixa)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", config =>
    {
        config.PermitLimit = 100;                       // Maximo de requisicoes permitidas
        config.Window = TimeSpan.FromMinutes(1);        // Janela de tempo (1 minuto)
    });
    // Retorna HTTP 429 (Too Many Requests) quando o limite e excedido
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Endpoint de health check para monitoramento (Kubernetes, load balancers, etc.)
builder.Services.AddHealthChecks();

// ============================================================
// RESILIENCIA COM POLLY
// ============================================================
// Configura um HttpClient resiliente com:
// - Retry: tenta novamente em caso de falha (ate 3x, com backoff exponencial)
// - Circuit Breaker: apos 10 falhas em 60s, para de chamar por 30s
// - Timeout: cancela requisicoes que demoram mais de 10s
builder.Services.AddHttpClient("resilient")
    .AddResilienceHandler("retry-circuit-breaker", resilience =>
    {
        resilience.AddRetry(new()
        {
            MaxRetryAttempts = 3,                       // Maximo de tentativas
            Delay = TimeSpan.FromMilliseconds(200),     // Atraso inicial entre tentativas
            BackoffType = DelayBackoffType.Exponential  // Backoff exponencial (200ms, 400ms, 800ms...)
        });
        resilience.AddCircuitBreaker(new()
        {
            MinimumThroughput = 10,                     // Minimo de requisicoes para avaliar
            BreakDuration = TimeSpan.FromSeconds(30),   // Tempo que o circuito fica aberto
            SamplingDuration = TimeSpan.FromSeconds(60) // Janela de amostragem
        });
        resilience.AddTimeout(TimeSpan.FromSeconds(10)); // Timeout por requisicao
    });

// Observabilidade (OpenTelemetry: tracing, metricas, logs)
builder.Services.AddNexusObservability("Nexus.Gateway.Api");
// Documentacao OpenAPI
builder.Services.AddOpenApi();

// ============================================================
// CONSTRUCAO DA APLICACAO E MIDDLEWARE PIPELINE
// ============================================================

var app = builder.Build();

// A ordem dos middlewares e importante! CORS e Rate Limiter devem vir antes do proxy.
app.UseCors();                                          // Habilita CORS
app.UseRateLimiter();                                   // Habilita limitacao de taxa
app.MapOpenApi();                                       // Endpoint /openapi/v1.json
app.MapScalarApiReference(options =>
{
    // Agrega os documentos OpenAPI de todos os microservicos (proxied pelo YARP)
    // para que o Scalar exiba todos os endpoints (incluindo login/register) em /scalar/v1
    options
        .AddDocument("v1", "Gateway")
        .AddDocument("users", "Users (Auth)", "/openapi/users.json")
        .AddDocument("catalog", "Catalog", "/openapi/catalog.json")
        .AddDocument("cart", "Cart", "/openapi/cart.json")
        .AddDocument("orders", "Orders", "/openapi/orders.json")
        .AddDocument("payments", "Payments", "/openapi/payments.json")
        .AddDocument("coupons", "Coupons", "/openapi/coupons.json");
});
app.MapReverseProxy();                                  // Habilita o proxy reverso (YARP)
app.MapHealthChecks("/health");                         // Endpoint /health para verificacao de saude

app.Run();
