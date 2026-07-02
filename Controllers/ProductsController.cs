using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RedisCacheAsideTest.Data;
using RedisCacheAsideTest.Models;
using RedisCacheAsideTest.Services;

namespace RedisCacheAsideTest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return Ok(product);
    }

    // READ ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _context.Products
                                .AsNoTracking()
                                .ToListAsync();

        return Ok(products);
    }

    // READ BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product updatedProduct)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        product.Name = updatedProduct.Name;
        product.Price = updatedProduct.Price;
        product.Stock = updatedProduct.Stock;

        await _context.SaveChangesAsync();

        return Ok(product);
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return Ok("Product deleted successfully");
    }

    // PROMOTION INVALIDATE
    [HttpPost("promotions/invalidate/{id}")]
    public async Task<IActionResult> Invalidate(int id, PromotionProductsCacheService cacheService)
    {
        await cacheService.InvalidateAsync(id);
        return NoContent();
    }

    // PROMOTION READ ALL
    [HttpGet("promotions")]
    public async Task<IActionResult> GetAllPromotion(PromotionProductsCacheService cacheService)
    {
        var products = await cacheService.GetPromotionProductsAsync();
        return Ok(products);
    }

    // PROMOTION READ ALL BY ID
    [HttpGet("promotions/{id}")]
    public async Task<IActionResult> GetAllPromotion(int id, PromotionProductsCacheService cacheService)
    {
        var product = await cacheService.GetPromotionProductByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    // UPDATE
    [HttpPut("promotions/{id}")]
    public async Task<IActionResult> UpdatePromotionFlag(    
        int id,
        bool isPromotion,
        AppDbContext db,
        PromotionProductsCacheService cacheService)
    {
        var product = await db.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        product.IsPromotion = isPromotion;

        await db.SaveChangesAsync();

        // Important for cache-aside: invalidate to avoid serving stale data
        await cacheService.InvalidateAsync(id);

        return Ok(product);
    }
}
