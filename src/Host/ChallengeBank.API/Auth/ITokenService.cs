using System.Security.Claims;

namespace ChallengeBank.API.Auth;

public interface ITokenService
{
    string GenerateToken(IEnumerable<Claim> claims);
}
