namespace RedisCacheAsideTest.Models;

/// <summary>
/// This model represents the database table.
/// EF Core automatically maps this class to a table called Products.
/// </summary>
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool IsPromotion { get; set; }
}
