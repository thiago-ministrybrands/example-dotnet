using StackExchange.Redis;

namespace Api.Caching;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private IDatabase Database => redis.GetDatabase();

    public async Task<string?> GetAsync(string key)
    {
        var value = await Database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public Task SetAsync(string key, string value, TimeSpan ttl) =>
        Database.StringSetAsync(key, value, ttl);
}
