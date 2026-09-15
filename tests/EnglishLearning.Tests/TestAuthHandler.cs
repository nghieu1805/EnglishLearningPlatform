using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Tests;

public sealed class TestAuthHandler
    : AuthenticationHandler<
        AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";

    public const string RoleHeader =
        "X-Test-Role";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions>
            options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(
                RoleHeader,
                out var roleHeader))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        string[] roles =
            roleHeader
                .ToString()
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    "test-user-id"),

                new(
                    ClaimTypes.Name,
                    "test@example.com"),

                new(
                    ClaimTypes.Email,
                    "test@example.com")
            };

        foreach (string role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        var identity =
            new ClaimsIdentity(
                claims,
                SchemeName);

        var principal =
            new ClaimsPrincipal(identity);

        var ticket =
            new AuthenticationTicket(
                principal,
                SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(
        AuthenticationProperties properties)
    {
        Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(
        AuthenticationProperties properties)
    {
        Response.StatusCode =
            StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    }
}