using System.Text.Json;
using ChallengeBank.Clients.Application.Abstractions;
using ChallengeBank.Clients.Application.DTOs;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace ChallengeBank.Clients.Infrastructure.Cache;

public sealed class RedisClientCache(IConnectionMultiplexer redis, IConfiguration configuration) : IClientCache
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null
    };

    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<ClientDto?> GetByIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        try
        {
            var key = Key(clientId);
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<ClientDto>(value!, JsonOptions);
        }
        catch (RedisConnectionException)
        {
            return null;
        }
        catch (RedisTimeoutException)
        {
            return null;
        }
    }

    public async Task SetByIdAsync(Guid clientId, ClientDto dto, TimeSpan ttl, CancellationToken cancellationToken)
    {
        try
        {
            var key = Key(clientId);
            var payload = JsonSerializer.Serialize(dto, JsonOptions);
            await _db.StringSetAsync(key, payload, expiry: ttl);
        }
        catch (RedisConnectionException)
        {
        }
        catch (RedisTimeoutException)
        {
        }
    }

    public async Task RemoveByIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        try
        {
            await _db.KeyDeleteAsync(Key(clientId));
        }
        catch (RedisConnectionException)
        {
        }
        catch (RedisTimeoutException)
        {
        }
    }

    private string Key(Guid id)
    {
        var prefix = configuration["Redis:KeyPrefix"];
        return string.IsNullOrWhiteSpace(prefix) ? $"clients:{id}" : $"{prefix}:clients:{id}";
    }
}

