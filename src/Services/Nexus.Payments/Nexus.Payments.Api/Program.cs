using Nexus.Payments.Application;
using Nexus.Payments.Infrastructure;
using Nexus.Shared.Observability;
using Scalar.AspNetCore;

// Cria o builder da aplicacao Web API
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONFIGURACAO DE SERVICOS (Dependency Injection)
// ============================================================

// Registra dependencias da Infraestrutura (DbContext, repositorios, clientes HTTP, etc.)
builder.Services.AddPaymentsInfrastructure(builder.Configuration);
// Adiciona observabilidade (OpenTelemetry, logs, metricas)
builder.Services.AddNexusObservability("Nexus.Payments.Api");
// Habilita documentacao OpenAPI
builder.Services.AddOpenApi();

// ============================================================
// CONSTRUCAO DA APLICACAO E MIDDLEWARE PIPELINE
// ============================================================

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// ============================================================
// ENDPOINTS DE PAGAMENTOS - Grupo /api/payments
// ============================================================

var payments = app.MapGroup("/api/payments");

// POST /api/payments - Processa um pagamento para um pedido
// Captura InvalidOperationException para erros de regra de negocio (ex: pedido ja pago)
payments.MapPost("/", async (Nexus.Payments.Application.ProcessPaymentRequest request, PaymentService service, CancellationToken ct) =>
{
    try
    {
        var payment = await service.ProcessPaymentAsync(request.OrderId, request, ct);
        return Results.Created($"/api/payments/{payment.Id}", payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// GET /api/payments/{id} - Busca um pagamento pelo ID
payments.MapGet("/{id:guid}", async (Guid id, PaymentService service, CancellationToken ct) =>
{
    var payment = await service.GetPaymentAsync(id, ct);
    return payment is null ? Results.NotFound() : Results.Ok(payment);
});

// GET /api/payments/order/{orderId} - Busca o pagamento associado a um pedido
payments.MapGet("/order/{orderId:guid}", async (Guid orderId, PaymentService service, CancellationToken ct) =>
{
    var payment = await service.GetByOrderIdAsync(orderId, ct);
    return payment is null ? Results.NotFound() : Results.Ok(payment);
});

// POST /api/payments/{id}/refund - Solicita reembolso de um pagamento
payments.MapPost("/{id:guid}/refund", async (Guid id, PaymentService service, CancellationToken ct) =>
{
    try
    {
        var payment = await service.RefundPaymentAsync(id, ct);
        return payment is null ? Results.NotFound() : Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Inicia a aplicacao e comeca a escutar requisicoes HTTP
app.Run();
