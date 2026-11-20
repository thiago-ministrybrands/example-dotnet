using System.Globalization;
using Api.Caching;
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ItemService(AppDbContext db, ICacheService cache)
{
    private const string CountCacheKey = "items:count";
    private static readonly TimeSpan CountCacheTtl = TimeSpan.FromSeconds(30);

    public async Task<Item> CreateAsync(string name)
    {
        var item = new Item { Name = name, CreatedAt = DateTimeOffset.UtcNow };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public Task<List<Item>> GetAllAsync() =>
        db.Items.OrderBy(i => i.Id).ToListAsync();

    public async Task<int> GetCountCachedAsync()
    {
        var cached = await cache.GetAsync(CountCacheKey);
        if (cached is not null && int.TryParse(cached, out var cachedCount))
        {
            return cachedCount;
        }

        var count = await db.Items.CountAsync();
        await cache.SetAsync(CountCacheKey, count.ToString(CultureInfo.InvariantCulture), CountCacheTtl);
        return count;
    }
}
