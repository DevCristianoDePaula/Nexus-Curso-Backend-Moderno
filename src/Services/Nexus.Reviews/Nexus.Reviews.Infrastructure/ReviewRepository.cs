using MongoDB.Driver;
using Nexus.Reviews.Application;
using Nexus.Reviews.Domain;

namespace Nexus.Reviews.Infrastructure;

public class ReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _reviews;
    private readonly IMongoCollection<ProductRating> _ratings;

    public ReviewRepository(IMongoDatabase database)
    {
        _reviews = database.GetCollection<Review>("reviews");
        _ratings = database.GetCollection<ProductRating>("productRatings");
    }

    public async Task AddAsync(Review review)
    {
        await _reviews.InsertOneAsync(review);
    }

    public async Task<List<Review>> GetByProductAsync(string productId, int page, int pageSize)
    {
        return await _reviews
            .Find(r => r.ProductId == productId)
            .SortByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<ProductRating?> GetProductRatingAsync(string productId)
    {
        return await _ratings
            .Find(r => r.ProductId == productId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateProductRatingAsync(string productId)
    {
        var allReviews = await _reviews.Find(r => r.ProductId == productId).ToListAsync();
        var rating = await _ratings
            .Find(r => r.ProductId == productId)
            .FirstOrDefaultAsync();

        if (rating is null)
        {
            rating = new ProductRating(productId);
            rating.Recalculate(allReviews);
            await _ratings.InsertOneAsync(rating);
        }
        else
        {
            rating.Recalculate(allReviews);
            await _ratings.ReplaceOneAsync(r => r.ProductId == productId, rating);
        }
    }
}
