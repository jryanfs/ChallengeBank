using System.Security.Claims;

namespace ChallengeBank.Api.Shared.Auth;

public interface ITokenService
{
    string GenerateToken(IEnumerable<Claim> claims);
}
