using Nexus.Orders.Application;
using Nexus.Orders.Infrastructure;
using Nexus.Shared.Observability;
using Scalar.AspNetCore;

// Cria o builder da aplicacao Web API
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONFIGURACAO DE SERVICOS (Dependency Injection)
// ============================================================

// Registra dependencias da Infraestrutura (DbContext, repositorios, etc.)
builder.Services.AddOrdersInfrastructure(builder.Configuration);
// Adiciona observabilidade (OpenTelemetry, logs, metricas)
builder.Services.AddNexusObservability("Nexus.Orders.Api");
// Habilita a documentacao OpenAPI
builder.Services.AddOpenApi();

// ============================================================
// CONSTRUCAO DA APLICACAO E MIDDLEWARE PIPELINE
// ============================================================

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// ============================================================
// ENDPOINTS DE PEDIDOS - Grupo /api/orders
// ============================================================

var orders = app.MapGroup("/api/orders");

// POST /api/orders - Cria um novo pedido a partir do carrinho
// Usa try/catch para capturar erros de negocio (ex: estoque insuficiente)
orders.MapPost("/", async (CreateOrderRequest request, OrderService service, CancellationToken ct) =>
{
    try
    {
        var order = await service.CreateOrderAsync(request, ct);
        return Results.Created($"/api/orders/{order.Id}", order);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// GET /api/orders/{id} - Busca um pedido pelo ID
orders.MapGet("/{id:guid}", async (Guid id, OrderService service, CancellationToken ct) =>
{
    var order = await service.GetOrderAsync(id, ct);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

// GET /api/orders/customer/{customerId} - Lista pedidos de um cliente com paginacao
orders.MapGet("/customer/{customerId}", async (string customerId, int page, int pageSize, OrderService service, CancellationToken ct) =>
{
    var result = await service.GetCustomerOrdersAsync(customerId, page, pageSize, ct);
    return Results.Ok(result);
});

// POST /api/orders/{id}/pay - Confirma o pagamento de um pedido
orders.MapPost("/{id:guid}/pay", async (Guid id, ConfirmPaymentRequest request, OrderService service, CancellationToken ct) =>
{
    try
    {
        var order = await service.ConfirmPaymentAsync(id, request.PaymentId, ct);
        return order is null ? Results.NotFound() : Results.Ok(order);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// POST /api/orders/{id}/cancel - Cancela um pedido existente
orders.MapPost("/{id:guid}/cancel", async (Guid id, CancelOrderRequest request, OrderService service, CancellationToken ct) =>
{
    try
    {
        var order = await service.CancelOrderAsync(id, request.Reason, ct);
        return order is null ? Results.NotFound() : Results.Ok(order);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Inicia a aplicacao e comeca a escutar requisicoes HTTP
app.Run();

// ============================================================
// DTOs DE REQUISICAO (Request)
// ============================================================

// DTO usado no endpoint POST /api/orders/{id}/pay
public class ConfirmPaymentRequest
{
    public string PaymentId { get; init; } = "";
}

// DTO usado no endpoint POST /api/orders/{id}/cancel
public class CancelOrderRequest
{
    public string Reason { get; init; } = "";
}
