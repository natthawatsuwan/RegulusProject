using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hangfire.Dashboard;
using Hangfire.PostgreSql.Properties;
using Microsoft.IdentityModel.Tokens;

namespace RegulusProject.Web;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Check for token in cookies or headers
        var token = httpContext.Request.Cookies["HangfireToken"];
        if (string.IsNullOrEmpty(token))
        {
            token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        }

        if (string.IsNullOrEmpty(token))
        {
            httpContext.Response.Redirect("/hangfirelogin");
            return false;
        }

        // Validate the JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("uba7036e7c9656a2e03c68d1be55c1033ccca5c88d4bbb3af4ca6ab687b4eb74");

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "gem.hangfire.Iss",
                ValidateAudience = true,
                ValidAudience = "gem.hangfire.Aud",
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            // Additional checks if needed, e.g., specific roles or claims
            //var jwtToken = (JwtSecurityToken)validatedToken;
            //var userClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

            return true; // JWT is valid and user is authenticated
        }
        catch
        {
            // Token validation failed
        }

        httpContext.Response.Redirect("/hangfirelogin");
        return false;
    }
}
