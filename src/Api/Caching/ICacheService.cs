namespace Api.Caching;

public interface ICacheService
{
    Task<string?> GetAsync(string key);

    Task SetAsync(string key, string value, TimeSpan ttl);
}
