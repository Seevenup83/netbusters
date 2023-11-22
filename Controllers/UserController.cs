using Microsoft.AspNetCore.Mvc;
using netbusters.Data;
using netbusters.Models;
using BCrypt.Net;
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
        private readonly JwtSettings _jwtSettings;

        public UserController(DatabaseContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost]
        public IActionResult Register(User registerRequest)
        {
            // Check if username already exists
            if (_context.Users.Any(u => u.Username == registerRequest.Username))
            {
                return BadRequest("Username is already taken.");
            }

            // Hash the password
            registerRequest.Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

            // Add user to database
            _context.Users.Add(registerRequest);
            _context.SaveChanges();

            // Return success response
            return Ok(new { Message = "Registration successful" });
        }

        /// <summary>
        /// Logs user into the system.
        /// </summary>        
        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = credentials,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
