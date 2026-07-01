using Microsoft.EntityFrameworkCore;

using StackExchange.Redis;

using RedisCacheAsideTest.Data;
using RedisCacheAsideTest.Models;
using RedisCacheAsideTest.Models.interfaces;
using RedisCacheAsideTest.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddScoped<IPromotionProductsCacheService, PromotionProductsCacheService>();

var sqliteConnectionString =
    builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=products.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnectionString));

var redisConnectionString =
    builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<PromotionProductsCacheService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();

    if (!db.Products.Any()) // Just for test purposes.
    {
        db.Products.AddRange(
            new Product { Name = "Tênis Runner X", Price = 299.90m, Stock = 100, IsPromotion = true },
            new Product { Name = "Camiseta Básica", Price = 49.90m, Stock = 200, IsPromotion = false },
            new Product { Name = "Fone Bluetooth", Price = 159.90m, Stock = 300, IsPromotion = true },
            new Product { Name = "Mochila Urbana", Price = 199.90m, Stock = 400, IsPromotion = false }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
