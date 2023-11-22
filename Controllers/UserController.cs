using Microsoft.AspNetCore.Mvc;
using netbusters.Data;
using netbusters.Models;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if username already exists
            if (_context.Users.Any(u => u.Username == registerRequest.Username))
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return BadRequest(ModelState);
            }

            // Hash the password here, after validation
            registerRequest.Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

            // Add user to the database
            _context.Users.Add(registerRequest);
            _context.SaveChanges();

            // Return success response
            return Ok(new { Message = "Registration successful" });
        }

        /// <summary>
        /// Logs user into the system.
        /// </summary>        
        [HttpPost("login")]
        public IActionResult Login(User loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.Users.SingleOrDefault(u => u.Username == loginRequest.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
            {
                ModelState.AddModelError("Password", "Invalid credentials.");
                return Unauthorized(ModelState);
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

        [Authorize]
        [HttpDelete("delete")]
        public IActionResult DeleteAccount()
        {
            // Extract username from the JWT token
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized($"User is not logged in. Extracted username: '{username}'");
            }

            // Find the user in the database
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Delete the user
            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok("User deleted successfully.");
        }
    }
}