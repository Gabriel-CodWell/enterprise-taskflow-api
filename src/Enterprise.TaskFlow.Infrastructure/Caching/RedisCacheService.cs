using System.Text.Json;
using Enterprise.TaskFlow.Domain.Interfaces;
using StackExchange.Redis;

namespace Enterprise.TaskFlow.Infrastructure.Caching;

/// <summary>
/// Redis-backed implementation of <see cref="ICacheService"/>.
/// Uses <see cref="IConnectionMultiplexer"/> (singleton) and System.Text.Json for serialization.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(value, JsonOptions);
        await _database.StringSetAsync(key, serialized, expiration);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }
}
