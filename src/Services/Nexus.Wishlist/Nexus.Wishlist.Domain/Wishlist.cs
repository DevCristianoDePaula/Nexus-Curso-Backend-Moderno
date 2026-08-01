namespace Nexus.Wishlist.Domain;

public class UserWishlist
{
    public string UserId { get; private set; }
    public List<WishlistItem> Items { get; private set; } = [];

    private UserWishlist() { }

    public UserWishlist(string userId)
    {
        UserId = userId;
    }

    public void AddItem(WishlistItem item)
    {
        if (Items.Any(i => i.ProductId == item.ProductId))
            throw new InvalidOperationException("Product already in wishlist");
        Items.Add(item);
    }

    public void RemoveItem(string productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
            Items.Remove(item);
    }

    public bool ContainsProduct(string productId) => Items.Any(i => i.ProductId == productId);
}
