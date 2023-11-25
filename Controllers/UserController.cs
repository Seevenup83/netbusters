//Controllers/UserController.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using netbusters.Data;
using netbusters.Models;
using netbusters.Common;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace netbusters.Controllers
{
    [ApiController]
    [Route("api/user")]
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
            var existingUser = _context.Users.Any(u => u.Username == registerRequest.Username);
            if (existingUser)
            {
                return BadRequest(ApiResponse.Failure("Username is already taken."));
            }

            registerRequest.Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
            _context.Users.Add(registerRequest);
            _context.SaveChanges();

            return Ok(ApiResponse.Success("Registration successful", new { Id = registerRequest.Id, Username = registerRequest.Username }));
        }

        /// <summary>
        /// Logs user into the system and retrieves authentication token.
        /// </summary>
        [HttpGet("login")]
        public IActionResult Login([FromQuery][Required] string username, [FromQuery][Required] string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(ApiResponse.Failure("Username and password are required."));
            }

            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return Unauthorized(ApiResponse.Failure("Invalid credentials."));
            }

            var token = GenerateJwtToken(user);
            return Ok(ApiResponse.Success("Login successful", new { Token = token }));
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Deletes the currently authenticated user's account.
        /// </summary>
        /// <remarks>
        /// This can only be done by the logged in user.
        /// </remarks>
        [Authorize]
        [HttpDelete("delete")]
        public IActionResult DeleteAccount()
        {
            var username = User.Identity?.Name;
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound(ApiResponse.Failure("User not found."));
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(ApiResponse.Success("User deleted successfully."));
        }

        /// <summary>
        /// Updates the currently authenticated user's information.
        /// </summary>
        /// <remarks>
        /// This can only be done by the logged in user.
        /// </remarks>
        [Authorize]
        [HttpPut]
        public IActionResult UpdateUser([FromBody] User updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Failure("Invalid user data."));
            }

            var username = User.Identity?.Name;
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound(ApiResponse.Failure("User not found."));
            }

            user.Username = updatedUser.Username;
            // Update other user details as necessary

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok(ApiResponse.Success("User updated successfully."));
        }

        /// <summary>
        /// Retrieves the currently authenticated user's information.
        /// </summary>
        /// <remarks>
        /// This can only be done by the logged in user.
        /// </remarks>
        [Authorize]
        [HttpGet]
        public IActionResult GetUser()
        {
            var username = User.Identity?.Name;
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound(ApiResponse.Failure("User not found."));
            }

            return Ok(ApiResponse.Success("User retrieved successfully.", new { user.Username }));
        }
    }
}
