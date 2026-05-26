using ChallengeBank.Transactions.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ChallengeBank.Transactions.Infrastructure.Cache;

public sealed class RedisTransferDuplicateGuard(
    IConnectionMultiplexer redis,
    IConfiguration configuration,
    ILogger<RedisTransferDuplicateGuard> logger) : ITransferDuplicateGuard
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<bool> TryRegisterAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        try
        {
            var key = TransferDuplicateKey.Build(senderUserId, receiverUserId, amount, Prefix);
            var registered = await _db.StringSetAsync(key, "1", Window, When.NotExists);
            if (!registered)
                logger.LogInformation(
                    "Transferência duplicada bloqueada. Key={Key}",
                    key);
            return registered;
        }
        catch (RedisException ex)
        {
            logger.LogError(ex, "Falha ao consultar Redis para anti-duplicata. Bloqueando transferência por segurança.");
            return false;
        }
    }

    public async Task ReleaseAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        try
        {
            var key = TransferDuplicateKey.Build(senderUserId, receiverUserId, amount, Prefix);
            await _db.KeyDeleteAsync(key);
        }
        catch (RedisConnectionException)
        {
        }
        catch (RedisTimeoutException)
        {
        }
    }

    private string? Prefix => configuration["Redis:KeyPrefix"];

    private TimeSpan Window => TimeSpan.FromMinutes(
        configuration.GetValue("Redis:TransferDuplicateWindowMinutes", 5));
}
