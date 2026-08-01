using Nexus.Reviews.Domain;

namespace Nexus.Reviews.Application;

public interface IReviewRepository
{
    Task AddAsync(Review review);
    Task<List<Review>> GetByProductAsync(string productId, int page, int pageSize);
    Task<ProductRating?> GetProductRatingAsync(string productId);
    Task UpdateProductRatingAsync(string productId);
}