namespace ChallengeBank.Api.Shared.Auth;

public sealed class AuthUserOptions
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}
