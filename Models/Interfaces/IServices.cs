using RedisCacheAsideTest.DTOs;

namespace RedisCacheAsideTest.Models.interfaces;

public interface IPromotionProductsCacheService
{
    Task<List<Product>> GetPromotionProductsAsync(CancellationToken ct = default);
    Task<Product?> GetPromotionProductByIdAsync(int id, CancellationToken ct = default);
    Task InvalidateAsync(int? productId = null);
}
