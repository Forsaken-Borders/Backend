using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ForsakenBorders.Backend.Models;
using ForsakenBorders.Backend.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ForsakenBorders.Backend.Security
{
    /// <summary>
    /// class to handle api_key security.
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// scheme name for authentication handler.
        /// </summary>
        public const string SchemeName = "ApiKey";
        private readonly BackendContext _context;

        /// <inheritdoc />
        public ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, BackendContext context) : base(options, logger, encoder, clock) => _context = context;

        /// <summary>
        /// verify that require api key header exist and handle authorization.
        /// </summary>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("api_key"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            if (!Guid.TryParse(Request.Headers["api_key"], out Guid authentication))
            {
                return AuthenticateResult.Fail("Invalid token.");
            }

            User user = await _context.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authentication);
            if (user is null)
            {
                return AuthenticateResult.Fail("Unknown token.");
            }
            else if (user.TokenExpiration < DateTime.UtcNow)
            {
                return AuthenticateResult.Fail("Expired token.");
            }

            Log log = new(authentication, CurrentUri, Request.Headers["User-Agent"]);
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            Claim[] claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, Scheme.Name),
                new Claim(ClaimTypes.Name, Scheme.Name),
            };
            ClaimsIdentity identity = new(claims, Scheme.Name);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
