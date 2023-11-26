// Services/TokenService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using netbusters.Models;
using netbusters.Data;
using Microsoft.AspNetCore.Mvc;

namespace netbusters.Services
{
    public class TokenService
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _configuration;

        public TokenService(DatabaseContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public (User User, ActionResult UnauthorizedResult) GetUserFromToken(HttpContext httpContext)
        {
            var authorizationHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer "))
            {
                return (null, new UnauthorizedObjectResult("User not authenticated."));
            }

            var tokenString = authorizationHeader.Substring("Bearer ".Length).Trim();
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(tokenString);

            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return (null, new UnauthorizedObjectResult("User not authenticated."));
            }

            var user = _context.Users.SingleOrDefault(u => u.Id == userId && u.Username == usernameClaim);
            if (user == null)
            {
                return (null, new UnauthorizedObjectResult("User not authenticated."));
            }

            return (user, null); // User is authenticated and valid
        }

        // Generates a JWT token for a given user.
        public string GenerateJwtToken(User user)
        {
            // Define claims to be included in the token.
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Create a signing key based on the secret key in the configuration.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Generate the token with specified claims, issuer, audience, and expiration.
            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds);

            // Return the serialized token.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}