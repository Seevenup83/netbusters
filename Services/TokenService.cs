// Services/TokenService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using netbusters.Models;
using netbusters.Data;

namespace netbusters.Services
{
    // This service class handles token-related operations.
    public class TokenService
    {
        // Database context to interact with the database.
        private readonly DatabaseContext _context;
        // Configuration to access application settings.
        private readonly IConfiguration _configuration;

        // Constructor for dependency injection.
        public TokenService(DatabaseContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Retrieves a user based on the JWT token in the request.
        public User GetUserFromToken(HttpContext httpContext)
        {
            // Extract the token from the Authorization header.
            var authorizationHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer "))
            {
                return null; // No token provided.
            }

            // Decode the token.
            var tokenString = authorizationHeader.Substring("Bearer ".Length).Trim();
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(tokenString);

            // Extract username and user ID claims from the token.
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value;

            // Validate and parse the user ID.
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return null; // Invalid token information.
            }

            // Retrieve the user from the database and validate the username.
            var user = _context.Users.SingleOrDefault(u => u.Id == userId);
            if (user != null && user.Username == usernameClaim)
            {
                return user;
            }

            return null; // User not found or username mismatch.
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