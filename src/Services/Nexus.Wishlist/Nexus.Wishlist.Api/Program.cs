using Nexus.Wishlist.Application;
using Nexus.Wishlist.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

?? "mongodb://nexus:Nexus%402026%23@localhost:27017";
var dbName = builder.Configuration["DatabaseName"] ?? "nexus_wishlist";

builder.Services.AddWishlistInfrastructure(mongoConn, dbName);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

var wishlist = app.MapGroup("/api/wishlist");

wishlist.MapGet("/{userId}", async (string userId, WishlistService service) =>
{
    var result = await service.GetWishlistAsync(userId);
    return result is not null ? Results.Ok(result) : Results.Ok(new { userId, items = new List<object>() });
});

wishlist.MapPost("/{userId}/items", async (string userId, AddWishlistItemRequest request, WishlistService service) =>
{
    await service.AddToWishlistAsync(userId, request.ProductId, request.ProductName, request.ProductPrice, request.ProductImageUrl);
    return Results.Created($"/api/wishlist/{userId}", new { message = "Item added to wishlist" });
});

wishlist.MapDelete("/{userId}/items/{productId}", async (string userId, string productId, WishlistService service) =>
{
    await service.RemoveFromWishlistAsync(userId, productId);
    return Results.NoContent();
});

wishlist.MapGet("/{userId}/contains/{productId}", async (string userId, string productId, WishlistService service) =>
{
    var contains = await service.IsInWishlistAsync(userId, productId);
    return Results.Ok(new { contains });
});

app.Run();

public record AddWishlistItemRequest(string ProductId, string ProductName, decimal ProductPrice, string ProductImageUrl);
