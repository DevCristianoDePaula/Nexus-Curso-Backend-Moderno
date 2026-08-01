using Nexus.Coupons.Application;
using Nexus.Coupons.Infrastructure;
using Nexus.Shared.Observability;
using Scalar.AspNetCore;

// Cria o builder da aplicacao Web API
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONFIGURACAO DE SERVICOS (Dependency Injection)
// ============================================================

// Registra dependencias da Infraestrutura (DbContext, repositorios, etc.)
builder.Services.AddCouponsInfrastructure(builder.Configuration);
// Adiciona observabilidade (OpenTelemetry, logs, metricas)
builder.Services.AddNexusObservability("Nexus.Coupons.Api");
// Habilita documentacao OpenAPI
builder.Services.AddOpenApi();

// ============================================================
// CONSTRUCAO DA APLICACAO E MIDDLEWARE PIPELINE
// ============================================================

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// ============================================================
// ENDPOINTS DE CUPONS - Grupo /api/coupons
// ============================================================

var coupons = app.MapGroup("/api/coupons");

// POST /api/coupons - Cria um novo cupom de desconto
// Captura ArgumentException para erros de validacao (ex: codigo ja existe, valor invalido)
coupons.MapPost("/", async (CreateCouponRequest request, CouponService service, CancellationToken ct) =>
{
    try
    {
        var coupon = await service.CreateCouponAsync(request, ct);
        return Results.Created($"/api/coupons/{coupon.Code}", coupon);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// GET /api/coupons/{code} - Busca um cupom pelo codigo
coupons.MapGet("/{code}", async (string code, CouponService service, CancellationToken ct) =>
{
    var coupon = await service.GetByCodeAsync(code, ct);
    return coupon is null ? Results.NotFound() : Results.Ok(coupon);
});

// POST /api/coupons/validate - Valida se um cupom pode ser aplicado a uma compra
// Retorna 200 com dados do desconto se valido, ou 400 com mensagem de erro se invalido
coupons.MapPost("/validate", async (ValidateCouponRequest request, CouponService service, CancellationToken ct) =>
{
    var result = await service.ValidateAndApplyAsync(request.Code, request.PurchaseAmount, request.CustomerId, request.CategoryId, ct);
    return result.IsValid ? Results.Ok(result) : Results.BadRequest(new { error = result.Error });
});

// POST /api/coupons/{code}/deactivate - Desativa um cupom (invalida o codigo)
coupons.MapPost("/{code}/deactivate", async (string code, CouponService service, CancellationToken ct) =>
{
    var coupon = await service.DeactivateAsync(code, ct);
    return coupon is null ? Results.NotFound() : Results.Ok(coupon);
});

// Inicia a aplicacao e comeca a escutar requisicoes HTTP
app.Run();

// ============================================================
// DTO DE REQUISICAO (Request)
// ============================================================

// DTO usado no endpoint POST /api/coupons/validate
public class ValidateCouponRequest
{
    public string Code { get; init; } = "";
    public decimal PurchaseAmount { get; init; }
    public string? CustomerId { get; init; }
    public string? CategoryId { get; init; }
}
