using System.Globalization;

namespace ChallengeBank.Transactions.Infrastructure.Cache;

internal static class TransferDuplicateKey
{
    public static string Build(Guid senderUserId, Guid receiverUserId, decimal amount, string? keyPrefix)
    {
        var normalized = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        var amountKey = normalized.ToString("F2", CultureInfo.InvariantCulture);
        var core = $"transfers:dedup:{senderUserId:N}:{receiverUserId:N}:{amountKey}";
        return string.IsNullOrWhiteSpace(keyPrefix) ? core : $"{keyPrefix}:{core}";
    }
}
