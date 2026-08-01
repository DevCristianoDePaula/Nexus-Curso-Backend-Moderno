using Nexus.Reviews.Domain;

namespace Nexus.Reviews.Application;

public class ReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Review> CreateReviewAsync(string productId, string userId, string userName, int rating, string comment)
    {
        var review = new Review(productId, userId, userName, rating, comment);
        await _reviewRepository.AddAsync(review);
        await _reviewRepository.UpdateProductRatingAsync(productId);
        return review;
    }

    public async Task<List<Review>> GetProductReviewsAsync(string productId, int page = 1, int pageSize = 20)
    {
        return await _reviewRepository.GetByProductAsync(productId, page, pageSize);
    }

    public async Task<ProductRating?> GetProductRatingAsync(string productId)
    {
        return await _reviewRepository.GetProductRatingAsync(productId);
    }
}