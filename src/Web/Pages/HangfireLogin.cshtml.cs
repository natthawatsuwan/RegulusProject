using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace RegulusProject.Web.Pages
{
    public class HangfireLoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; } = default!;
        [BindProperty]
        public string Password { get; set; } = default!;

        public IActionResult OnPost()
        {
            // Simple authentication check; replace with your logic
            if (Username == "admin" && Password == "hangfireP@ssw0rd")
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("uba7036e7c9656a2e03c68d1be55c1033ccca5c88d4bbb3af4ca6ab687b4eb74");

                // Create JWT token with claims
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, Username) }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = "gem.hangfire.Iss",
                    Audience = "gem.hangfire.Aud",
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);
                var domainName = Request.Host.Host;
                // Save the token to a cookie or local storage
                Response.Cookies.Append("HangfireToken", tokenString, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Ensure this is `false` for `http` or `true` for `https`
                    SameSite = SameSiteMode.Lax,
                    Domain = domainName
                });

                return Redirect("/hangfire"); // Redirect to Hangfire dashboard
            }

            ViewData["Error"] = "Invalid credentials.";
            return Page();
        }
    }
}
