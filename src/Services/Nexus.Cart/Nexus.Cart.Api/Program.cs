using Nexus.Cart.Application;
using Nexus.Cart.Infrastructure;
using Nexus.Shared.Observability;
using Scalar.AspNetCore;

// Cria o builder da aplicacao Web API
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONFIGURACAO DE SERVICOS (Dependency Injection)
// ============================================================

// Registra as dependencias da Infraestrutura (DbContext, repositorios, etc.)
builder.Services.AddCartInfrastructure(builder.Configuration);
// Adiciona observabilidade (OpenTelemetry, logs estruturados, metricas)
builder.Services.AddNexusObservability("Nexus.Cart.Api");
// Gera documentacao OpenAPI automaticamente para os endpoints
builder.Services.AddOpenApi();

// ============================================================
// CONSTRUCAO DA APLICACAO E MIDDLEWARE PIPELINE
// ============================================================

var app = builder.Build();

// Disponibiliza a documentacao OpenAPI e a interface interativa Scalar
app.MapOpenApi();
app.MapScalarApiReference();

// ============================================================
// ENDPOINTS DO CARRINHO - Grupo /api/cart
// ============================================================

var cart = app.MapGroup("/api/cart");

// GET /api/cart/{userId} - Retorna o carrinho atual do usuario
cart.MapGet("/{userId}", async (string userId, CartService service, CancellationToken ct) =>
{
    var result = await service.GetCartAsync(userId, ct);
    return Results.Ok(result);
});

// POST /api/cart/{userId}/items - Adiciona um item ao carrinho
cart.MapPost("/{userId}/items", async (string userId, AddCartItemRequest request, CartService service, CancellationToken ct) =>
{
    await service.AddItemAsync(userId, request, ct);
    return Results.Ok();
});

// DELETE /api/cart/{userId}/items/{productId} - Remove um item especifico do carrinho
cart.MapDelete("/{userId}/items/{productId}", async (string userId, string productId, CartService service, CancellationToken ct) =>
{
    await service.RemoveItemAsync(userId, productId, ct);
    return Results.Ok();
});

// PUT /api/cart/{userId}/items/{productId}/quantity/{quantity} - Atualiza a quantidade de um item
cart.MapPut("/{userId}/items/{productId}/quantity/{quantity:int}", async (string userId, string productId, int quantity, CartService service, CancellationToken ct) =>
{
    await service.UpdateQuantityAsync(userId, productId, quantity, ct);
    return Results.Ok();
});

// DELETE /api/cart/{userId} - Limpa todo o carrinho do usuario
cart.MapDelete("/{userId}", async (string userId, CartService service, CancellationToken ct) =>
{
    await service.ClearCartAsync(userId, ct);
    return Results.Ok();
});

// Inicia a aplicacao e comeca a escutar requisicoes HTTP
app.Run();
