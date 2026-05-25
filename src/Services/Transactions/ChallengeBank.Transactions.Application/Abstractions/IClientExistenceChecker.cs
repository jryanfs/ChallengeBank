namespace ChallengeBank.Transactions.Application.Abstractions;

public interface IClientExistenceChecker
{
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}
