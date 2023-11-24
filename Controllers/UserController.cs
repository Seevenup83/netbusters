//Controllers/UserController.cs
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
            // Check if username already exists
            var existingUsers = _context.Users
                .Where(u => u.Username == registerRequest.Username)
                .Select(u => new { u.Id, u.Username })
                .ToList();

            if (existingUsers.Any())
            {
                return BadRequest(new ApiResponse("Username is already taken.", existingUsers));
            }

            // Hash the password here, after validation
            registerRequest.Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

            // Add user to the database
            _context.Users.Add(registerRequest);
            _context.SaveChanges();

            var responseData = new { Id = registerRequest.Id, Username = registerRequest.Username };
            return Ok(new ApiResponse("Registration successful", responseData));
        
        }

        /// <summary>
        /// Logs user into the system.
        /// </summary>        
        [HttpPost("login")]
        public IActionResult Login(User loginRequest)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == loginRequest.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
            {
                return Unauthorized(new ApiResponse("Invalid credentials.", null));
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
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Ensure this is the user's ID
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
                return Unauthorized(new ApiResponse("User is not logged in.", username));
            }

            // Find the user in the database
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound(new ApiResponse("User not found.", username));
            }

            // Delete the user
            var deletedUserId = user.Id;
            _context.Users.Remove(user);
            _context.SaveChanges();

            var responseData = new { Id = deletedUserId, Username = user.Username };
            return Ok(new ApiResponse("User deleted successfully.", responseData));
        }

        /// <summary>
        /// Updates the currently authenticated user's information.
        /// </summary>
        [Authorize]
        [HttpPut]
        public IActionResult UpdateUser([FromBody] User updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Invalid user data.", null));
            }

            var username = User.Identity?.Name;
            var existingUser = _context.Users.SingleOrDefault(u => u.Username == username);
            if (existingUser == null)
            {
                return NotFound(new ApiResponse("User not found.", null));
            }

            // Prevent updating to an existing username
            if (_context.Users.Any(u => u.Username == updatedUser.Username && u.Id != existingUser.Id))
            {
                return BadRequest(new ApiResponse("Username already taken.", null));
            }

            // Update user properties, be careful with sensitive information like passwords
            existingUser.Username = updatedUser.Username;
            // Update other properties as necessary

            _context.Users.Update(existingUser);
            _context.SaveChanges();

            var responseData = new { Id = existingUser.Id, Username = existingUser.Username };
            return Ok(new ApiResponse("User updated successfully.", responseData));
        }

        /// <summary>
        /// Retrieves the currently authenticated user's information.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult GetUser()
        {
            var username = User.Identity?.Name;
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound(new ApiResponse("User not found.", null));
            }

            var responseData = new { Id = user.Id, Username = user.Username };
            return Ok(new ApiResponse("User retrieved successfully.", responseData));
        }
    }
}