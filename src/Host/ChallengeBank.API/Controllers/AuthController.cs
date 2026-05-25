using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChallengeBank.API.Api;
using ChallengeBank.API.Auth;
using ChallengeBank.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Autenticação")]
public sealed class AuthController(IAuthService authService, ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var auth = authService.Authenticate(request.Username, request.Password);

        if (auth is null)
            return this.ToJsonResponse(StatusCodes.Status401Unauthorized, ApiMessages.LoginInvalid);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, auth.Username),
            new(JwtRegisteredClaimNames.UniqueName, auth.Username),
            new("role", auth.Role)
        };

        var token = tokenService.GenerateToken(claims);

        var data = new
        {
            accessToken = token,
            username = auth.Username,
            role = auth.Role,
            expiresAtUtc = DateTime.UtcNow.AddHours(1)
        };

        return this.ToJsonResponse(StatusCodes.Status200OK, ApiMessages.LoginSuccess, data);
    }
}

public sealed record LoginRequest(string Username, string Password);
