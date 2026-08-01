using Nexus.Wishlist.Domain;

namespace Nexus.Wishlist.Application;

public interface IWishlistRepository
{
    Task<UserWishlist?> GetByUserAsync(string userId);
    Task AddItemAsync(string userId, WishlistItem item);
    Task RemoveItemAsync(string userId, string productId);
    Task<bool> ContainsAsync(string userId, string productId);
}
