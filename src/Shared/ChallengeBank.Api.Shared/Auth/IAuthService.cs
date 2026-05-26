namespace ChallengeBank.Api.Shared.Auth;

public interface IAuthService
{
    AuthResult? Authenticate(string username, string password);
}

public sealed record AuthResult(string Username, string Role);
