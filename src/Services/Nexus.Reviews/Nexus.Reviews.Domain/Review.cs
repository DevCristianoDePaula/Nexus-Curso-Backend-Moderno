namespace Nexus.Reviews.Domain;

public class Review
{
    public Guid Id { get; private set; }
    public string ProductId { get; private set; }
    public string UserId { get; private set; }
    public string UserName { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Review() { }

    public Review(string productId, string userId, string userName, int rating, string comment)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        UserId = userId;
        UserName = userName;
        SetRating(rating);
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(int rating, string comment)
    {
        SetRating(rating);
        Comment = comment;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");
        Rating = rating;
    }
}