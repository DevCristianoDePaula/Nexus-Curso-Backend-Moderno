namespace Nexus.Wishlist.Domain;

public class WishlistItem
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public string ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal ProductPrice { get; private set; }
    public string ProductImageUrl { get; private set; }
    public DateTime AddedAt { get; private set; }

    private WishlistItem() { }

    public WishlistItem(string userId, string productId, string productName, decimal productPrice, string productImageUrl)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ProductId = productId;
        ProductName = productName;
        ProductPrice = productPrice;
        ProductImageUrl = productImageUrl;
        AddedAt = DateTime.UtcNow;
    }
}
