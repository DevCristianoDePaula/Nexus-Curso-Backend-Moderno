using Nexus.Wishlist.Domain;

namespace Nexus.Wishlist.Application;

public class WishlistService
{
    private readonly IWishlistRepository _repository;

    public WishlistService(IWishlistRepository repository)
    {
        _repository = repository;
    }

    public async Task AddToWishlistAsync(string userId, string productId, string productName, decimal productPrice, string productImageUrl)
    {
        var item = new WishlistItem(userId, productId, productName, productPrice, productImageUrl);
        await _repository.AddItemAsync(userId, item);
    }

    public async Task RemoveFromWishlistAsync(string userId, string productId)
    {
        await _repository.RemoveItemAsync(userId, productId);
    }

    public async Task<UserWishlist?> GetWishlistAsync(string userId)
    {
        return await _repository.GetByUserAsync(userId);
    }

    public async Task<bool> IsInWishlistAsync(string userId, string productId)
    {
        return await _repository.ContainsAsync(userId, productId);
    }
}
