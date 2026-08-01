using Nexus.Catalog.Application;
using Nexus.Catalog.Infrastructure;
using Nexus.Shared.Observability;
using Scalar.AspNetCore;

// Cria o builder da aplicacao Web API, responsavel por configurar servicos e o pipeline HTTP
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONFIGURACAO DE SERVICOS (Dependency Injection)
// ============================================================

// Registra as dependencias da camada de Infraestrutura (Ex: DbContext, repositorios)
builder.Services.AddCatalogInfrastructure(builder.Configuration);
// Registra o servico de aplicacao (caso de uso) com escopo por requisicao
builder.Services.AddScoped<CatalogService>();
// Adiciona servicos de observabilidade (OpenTelemetry, logs, metricas)
builder.Services.AddNexusObservability("Nexus.Catalog.Api");
// Habilita a geracao automatica da documentacao OpenAPI (Swagger)
builder.Services.AddOpenApi();
// Endpoint de health check para monitoramento
builder.Services.AddHealthChecks();

// ============================================================
// CONSTRUCAO DA APLICACAO E MIDDLEWARE PIPELINE
// ============================================================

var app = builder.Build();

// Disponibiliza o endpoint /openapi/v1.json e a interface Scalar para testar as APIs
app.MapOpenApi();
app.MapScalarApiReference();
app.MapHealthChecks("/health");

// ============================================================
// ENDPOINTS DO CATALOGO - Grupo /api/products
// ============================================================

// MapGroup cria um prefixo comum ("/api/products") para todas as rotas do grupo
var products = app.MapGroup("/api/products");

// POST /api/products - Cria um novo produto
// Usa try/catch para capturar erros de negocio (InvalidOperationException)
// e retornar HTTP 400 com a mensagem do erro
products.MapPost("/", async (CreateProductRequest request, CatalogService service, CancellationToken ct) =>
{
    try
    {
        var product = await service.CreateProductAsync(request, ct);
        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// GET /api/products/{id} - Busca um produto pelo ID (Guid)
// Se nao encontrado, retorna HTTP 404
products.MapGet("/{id:guid}", async (Guid id, CatalogService service, CancellationToken ct) =>
{
    var product = await service.GetProductAsync(id, ct);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

// GET /api/products/category/{categoryId} - Lista produtos por categoria com paginacao
products.MapGet("/category/{categoryId}", async (string categoryId, int page, int pageSize, CatalogService service, CancellationToken ct) =>
{
    var products = await service.GetByCategoryAsync(categoryId, page, pageSize, ct);
    return Results.Ok(products);
});

// GET /api/products/search?q=termo - Busca textual de produtos
// Valida se o termo de busca foi informado; retorna HTTP 400 se estiver vazio
products.MapGet("/search", async (string q, int page, int pageSize, CatalogService service, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest(new { error = "Search term 'q' is required" });

    var products = await service.SearchAsync(q, page, pageSize, ct);
    return Results.Ok(products);
});

// Inicia a aplicacao e comeca a escutar requisicoes HTTP na porta configurada
app.Run();