using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ForsakenBorders.Backend.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ForsakenBorders.Backend.Api.Auth
{
    /// <summary>
    /// Validates the user through the Authorization header, and runs checks on the token.
    /// </summary>
    public class TokenAuth : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// The database context. Used for retrieving and validating the user.
        /// </summary>
        private readonly BackendContext _context;

        public TokenAuth(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, BackendContext context) : base(options, logger, encoder, clock)
        {
            // No idea what those unused parameteres are for, but they are required.
            // If I could get rid of them, I would.
            _context = context;
        }

        /// <summary>
        /// Checks if the user passed an authorization header and if the token is valid.
        /// </summary>
        /// <returns>
        /// - Missing token: if the user did not pass an authorization header.
        /// - Invalid token: if the token format is invalid.
        /// - Expired token: if the token is expired.
        /// - Unknown token: if the user does not exist.
        /// </returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorizationToken = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationToken?.Trim()))
            {
                return AuthenticateResult.Fail("Missing token.");
            }

            if (!Guid.TryParse(authorizationToken, out Guid authorization))
            {
                return AuthenticateResult.Fail("Invalid token.");
            }

            User user = await _context.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authorization);
            if (user is null)
            {
                return AuthenticateResult.Fail("Unknown token.");
            }
            else if (user.TokenExpiration < DateTime.UtcNow)
            {
                return AuthenticateResult.Fail("Expired token.");
            }

            Log log = new(authorization, CurrentUri, Request.Headers["User-Agent"]);
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            Claim[] claims = new[] { new Claim(ClaimTypes.Name, "TokenAuthentication") };
            ClaimsIdentity identity = new(claims, Scheme.Name);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}