using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;

namespace TravelWise.Api.Auth;

public class ServiceTokenAuthOptions : AuthenticationSchemeOptions
{
    public string ExpectedToken { get; set; } = string.Empty;
}

public class ServiceTokenAuthHandler : AuthenticationHandler<ServiceTokenAuthOptions>
{
    public const string SchemeName = "ServiceToken";

    public ServiceTokenAuthHandler(
        IOptionsMonitor<ServiceTokenAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header))
            return Task.FromResult(AuthenticateResult.NoResult());

        var value = header.ToString();
        if (!value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Missing bearer prefix"));

        var token = value.Substring("Bearer ".Length).Trim();
        var expected = Options.ExpectedToken;

        if (string.IsNullOrEmpty(expected))
            return Task.FromResult(AuthenticateResult.Fail("Service token not configured"));

        // Constant-time comparison to avoid timing attacks
        if (!CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(token),
                System.Text.Encoding.UTF8.GetBytes(expected)))
            return Task.FromResult(AuthenticateResult.Fail("Invalid service token"));

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "ai-service"),
            new Claim(ClaimTypes.Role, "Service")
        }, SchemeName);

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}