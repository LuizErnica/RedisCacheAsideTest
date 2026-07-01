using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RedisCacheAsideTest.Data;
using RedisCacheAsideTest.Models;
using RedisCacheAsideTest.Models.interfaces;
using StackExchange.Redis;

namespace RedisCacheAsideTest.Services;

public class PromotionProductsCacheService : IPromotionProductsCacheService
{
    // Unique key for the complete list of products on sale
    private const string ListCacheKey = "products:promotion:list";

    // Key per individual product (useful if you also want to
    // fetch/invalidate a specific product)
    private const string ItemCacheKeyPrefix = "products:promotion:item:";

    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private readonly AppDbContext _db;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<PromotionProductsCacheService> _logger;

    public PromotionProductsCacheService(
        AppDbContext db,
        IConnectionMultiplexer redis,
        ILogger<PromotionProductsCacheService> logger)
    {
        _db = db;
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Cache-aside: attempts to read from Redis; if not found (cache miss),
    /// fetches from SQLite, writes to Redis with a 5-minute TTL, and returns.
    /// </summary>
    public async Task<List<Product>> GetPromotionProductsAsync(CancellationToken ct = default)
    {
        var cache = _redis.GetDatabase();

        RedisValue cached = await cache.StringGetAsync(ListCacheKey);
        if (cached.HasValue)
        {
            _logger.LogInformation("Cache HIT: {Key}", ListCacheKey);
            return JsonSerializer.Deserialize<List<Product>>((string)cached!) ?? new List<Product>();
        }

        _logger.LogInformation("Cache MISS: {Key} — retrieve data from SQLite", ListCacheKey);

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsPromotion)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);

        var json = JsonSerializer.Serialize(products);
        await cache.StringSetAsync(ListCacheKey, json, Ttl);

        return products;
    }

    /// <summary>
    /// Cache-aside for a single product on sale, by ID.
    /// </summary>
    public async Task<Product?> GetPromotionProductByIdAsync(int id, CancellationToken ct = default)
    {
        var cache = _redis.GetDatabase();
        var key = ItemCacheKeyPrefix + id;

        RedisValue cached = await cache.StringGetAsync(key);
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<Product>((string)cached!);
        }

        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsPromotion, ct);

        if (product is null)
        {
            return null;
        }

        await cache.StringSetAsync(key, JsonSerializer.Serialize(product), Ttl);
        return product;
    }

    /// <summary>
    /// Invalidates the cache. Call this whenever a product is created,
    /// updated, or has its promotion status changed, to avoid
    /// serving stale data until the TTL naturally expires.
    /// </summary>
    public async Task InvalidateAsync(int? productId = null)
    {
        var cache = _redis.GetDatabase();
        await cache.KeyDeleteAsync(ListCacheKey);

        if (productId.HasValue)
        {
            await cache.KeyDeleteAsync(ItemCacheKeyPrefix + productId.Value);
        }

        _logger.LogInformation("Cache invalidated (productId={ProductId})", productId);
    }
}
