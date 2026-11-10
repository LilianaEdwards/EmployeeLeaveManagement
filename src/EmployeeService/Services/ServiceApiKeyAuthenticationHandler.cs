
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EmployeeService.Services;

public class ServiceApiKeyAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ServiceApiKey";

    private const string HeaderName = "X-Service-Api-Key";

    private readonly IConfiguration _configuration;

    public ServiceApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var expectedApiKey = _configuration[
            "ServiceAuthentication:ApiKey"];

        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            Logger.LogError(
                "Service API key is not configured.");

            return Task.FromResult(
                AuthenticateResult.Fail(
                    "Service authentication is not configured."));
        }

        if (!Request.Headers.TryGetValue(
                HeaderName, out var providedValues)
            || providedValues.Count != 1)
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var providedApiKey = providedValues.ToString();

        var expectedBytes = Encoding.UTF8.GetBytes(expectedApiKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedApiKey);

        var isValid = CryptographicOperations.FixedTimeEquals(
            expectedBytes,
            providedBytes);

        if (!isValid)
        {
            return Task.FromResult(
                AuthenticateResult.Fail(
                    "Invalid service API key."));
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                "leave-service"),

            new Claim(
                ClaimTypes.Name,
                "LeaveService"),

            new Claim(
                ClaimTypes.Role,
                "Service")
        };

        var identity = new ClaimsIdentity(
            claims,
            SchemeName);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
