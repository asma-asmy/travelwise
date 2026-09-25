using TravelWise.Domain.Entities;

namespace TravelWise.Application.Common.Interfaces;

public interface IJwtTokenService
{
    /// <summary>Creates a signed JWT access token for the user, embedding role claims.</summary>
    (string token, DateTime expiresAtUtc) CreateAccessToken(User user, IEnumerable<string> roles);

    /// <summary>Creates a cryptographically random refresh token string.</summary>
    string CreateRefreshTokenString();
}