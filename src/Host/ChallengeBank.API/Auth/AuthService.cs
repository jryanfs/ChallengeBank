namespace ChallengeBank.API.Auth;

public sealed class AuthService(IConfiguration configuration) : IAuthService
{
    public AuthResult? Authenticate(string username, string password)
    {
        var users = configuration.GetSection("Auth:Users").Get<List<AuthUserOptions>>() ?? [];

        var user = users.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)
            && u.Password == password);

        return user is null ? null : new AuthResult(user.Username, user.Role);
    }
}
