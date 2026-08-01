using MongoDB.Driver;
using Nexus.Wishlist.Application;
using Nexus.Wishlist.Domain;

namespace Nexus.Wishlist.Infrastructure;

public class WishlistRepository : IWishlistRepository
{
    private readonly IMongoCollection<UserWishlist> _wishlists;

    public WishlistRepository(IMongoDatabase database)
    {
        _wishlists = database.GetCollection<UserWishlist>("wishlists");
    }

    public async Task<UserWishlist?> GetByUserAsync(string userId)
    {
        return await _wishlists.Find(w => w.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task AddItemAsync(string userId, WishlistItem item)
    {
        var wishlist = await GetByUserAsync(userId);
        if (wishlist is null)
        {
            wishlist = new UserWishlist(userId);
            wishlist.AddItem(item);
            await _wishlists.InsertOneAsync(wishlist);
        }
        else
        {
            if (!wishlist.ContainsProduct(item.ProductId))
            {
                wishlist.AddItem(item);
                await _wishlists.ReplaceOneAsync(w => w.UserId == userId, wishlist);
            }
        }
    }

    public async Task RemoveItemAsync(string userId, string productId)
    {
        var wishlist = await GetByUserAsync(userId);
        if (wishlist is not null)
        {
            wishlist.RemoveItem(productId);
            await _wishlists.ReplaceOneAsync(w => w.UserId == userId, wishlist);
        }
    }

    public async Task<bool> ContainsAsync(string userId, string productId)
    {
        var wishlist = await GetByUserAsync(userId);
        return wishlist?.ContainsProduct(productId) ?? false;
    }
}
