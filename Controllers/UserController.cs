using Microsoft.AspNetCore.Mvc;
using netbusters.Data;
using netbusters.Models;
using netbusters.Utilities;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace netbusters.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _configuration;
        public UserController(DatabaseContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost]
        public IActionResult Register(User registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(1, "Bad Request."));
            }

            // Check if username already exists
            if (_context.Users.Any(u => u.Username == registerRequest.Username))
            {
                return BadRequest(new ApiResponse(1, "Username is already taken."));
            }

            // Hash the password here, after validation
            registerRequest.Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

            // Add user to the database
            _context.Users.Add(registerRequest);
            _context.SaveChanges();

            return Ok(new ApiResponse(0, "Registration successful"));
        }

        /// <summary>
        /// Logs user into the system.
        /// </summary>        
        [HttpPost("login")]
        public IActionResult Login(User loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(1, "Validation error"));
            }

            var user = _context.Users.SingleOrDefault(u => u.Username == loginRequest.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
            {
                return Unauthorized(new ApiResponse(1, "Invalid credentials."));
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var secretKey = _configuration["JwtSettings:SecretKey"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = credentials,
                Issuer = issuer,
                Audience = audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Deletes the currently authenticated user's account.
        /// </summary>     
        [Authorize]
        [HttpDelete("delete")]
        public IActionResult DeleteAccount()
        {
            // Extract username from the JWT token
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new ApiResponse(1, $"User is not logged in. Extracted username: '{username}'"));
            }

            // Find the user in the database
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound(new ApiResponse(1, "User not found."));
            }

            // Delete the user
            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new ApiResponse(0, "User deleted successfully."));
        }
    }
}