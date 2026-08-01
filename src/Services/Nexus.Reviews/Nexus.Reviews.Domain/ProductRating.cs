namespace Nexus.Reviews.Domain;

public class ProductRating
{
    public string ProductId { get; private set; }
    public double AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public DateTime LastUpdated { get; private set; }

    private ProductRating() { }

    public ProductRating(string productId)
    {
        ProductId = productId;
        AverageRating = 0;
        TotalReviews = 0;
        LastUpdated = DateTime.UtcNow;
    }

    public void Recalculate(IEnumerable<Review> reviews)
    {
        var list = reviews.ToList();
        TotalReviews = list.Count;
        AverageRating = TotalReviews > 0 ? list.Average(r => r.Rating) : 0;
        LastUpdated = DateTime.UtcNow;
    }
}