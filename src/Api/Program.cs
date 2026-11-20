using Api.Caching;
using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Port=5432;Database=exampledb;Username=exampledb;Password=exampledb";
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? "localhost:6379";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

builder.Services.AddScoped<ItemService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/items", async (ItemService items) =>
    Results.Ok(await items.GetAllAsync()));

app.MapPost("/items", async (CreateItemRequest request, ItemService items) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new { error = "Name is required." });
    }

    var created = await items.CreateAsync(request.Name);
    return Results.Created($"/items/{created.Id}", created);
});

app.MapGet("/items/count", async (ItemService items) =>
    Results.Ok(new { count = await items.GetCountCachedAsync() }));

app.Run();
