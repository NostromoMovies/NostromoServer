using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Server.Database.Repositories;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Nostromo.Server.API.Authentication
{
    public class CustomAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AuthTokenRepository _authTokens;

        public CustomAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            AuthTokenRepository authTokens)
            : base(options, logger, encoder, clock)
        {
            _authTokens = authTokens;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Get apikey from header or query
            var authkeys = Request.Headers["apikey"]
                .Union(Request.Query["apikey"])
                .ToList();

            if (authkeys.Count == 0)
            {
                return Task.FromResult(AuthenticateResult.Fail("Cannot read authorization header or query."));
            }

            // Try to find a valid token
            var auth = authkeys.Select(apikey => _authTokens.GetByToken(token: apikey?.Trim()))
                .FirstOrDefault(token => token != null);

            if (auth == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authentication key"));
            }

            var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "user"),
            new(ClaimTypes.NameIdentifier, auth.UserId.ToString()),
            new(ClaimTypes.AuthenticationMethod, "apikey")
        };

            if (auth.User.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
