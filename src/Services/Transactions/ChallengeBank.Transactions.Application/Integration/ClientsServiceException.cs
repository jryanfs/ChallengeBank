namespace ChallengeBank.Transactions.Application.Integration;

public sealed class ClientsServiceException(string message, Exception? innerException = null)
    : Exception(message, innerException);
