using Nexus.Reviews.Application;
using Nexus.Reviews.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

?? "mongodb://nexus:Nexus%402026%23@localhost:27017";
var dbName = builder.Configuration["DatabaseName"] ?? "nexus_reviews";

builder.Services.AddReviewsInfrastructure(mongoConn, dbName);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

var reviews = app.MapGroup("/api/reviews");

reviews.MapPost("/", async (CreateReviewRequest request, ReviewService service) =>
{
    var review = await service.CreateReviewAsync(request.ProductId, request.UserId, request.UserName, request.Rating, request.Comment);
    return Results.Created($"/api/reviews/{review.Id}", review);
});

reviews.MapGet("/product/{productId}", async (string productId, int page, int pageSize, ReviewService service) =>
{
    var result = await service.GetProductReviewsAsync(productId, page, pageSize);
    return Results.Ok(result);
});

reviews.MapGet("/product/{productId}/rating", async (string productId, ReviewService service) =>
{
    var rating = await service.GetProductRatingAsync(productId);
    return rating is not null ? Results.Ok(rating) : Results.Ok(new { productId, averageRating = 0, totalReviews = 0 });
});

app.Run();

public record CreateReviewRequest(string ProductId, string UserId, string UserName, int Rating, string Comment);
