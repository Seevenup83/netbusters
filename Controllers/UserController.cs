// Controllers/UserController.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using netbusters.Data;
using netbusters.Models;
using netbusters.Services;

namespace netbusters.Controllers
{
    // ApiController attribute indicates that the controller responds to web API requests.
    // Route attribute defines the routing pattern for the controller's actions.
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        // Database context for accessing the database.
        private readonly DatabaseContext _context;
        // Service for handling token-related operations.
        private readonly TokenService _tokenService;

        // Constructor for injecting dependencies.
        public UserController(DatabaseContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// 
        [HttpPost]
        public IActionResult Register(User registerRequest)
        {
            // Check if the username already exists in the database.
            var existingUser = _context.Users.Any(u => u.Username == registerRequest.Username);
            if (existingUser)
            {
                // Return BadRequest if the username is already taken.
                return BadRequest(ApiResponseService.Failure("Username is already taken."));
            }

            // Hash the password before storing it in the database.
            registerRequest.Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
            _context.Users.Add(registerRequest);
            _context.SaveChanges();

            // Return success response with the newly created user's ID and username.
            return Ok(ApiResponseService.Success("Registration successful", new { Id = registerRequest.Id, Username = registerRequest.Username }));
        }

        /// <summary>
        /// Logs user into the system and retrieves authentication token.
        /// </summary>
        [HttpGet("login")]
        public IActionResult Login([FromQuery][Required] string username, [FromQuery][Required] string password)
        {
            // Validate username and password input.
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(ApiResponseService.Failure("Username and password are required."));
            }

            // Find the user by username and verify the password.
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                // Return Unauthorized if credentials are invalid.
                return Unauthorized(ApiResponseService.Failure("Invalid credentials."));
            }

            // Generate JWT token for the authenticated user.
            var token = _tokenService.GenerateJwtToken(user);
            return Ok(ApiResponseService.Success("Login successful", new { Token = token }));
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
            // Retrieve the current user based on the token.
            var (user, unauthorizedResult) = _tokenService.GetUserFromToken(HttpContext);
            if (unauthorizedResult != null) return Unauthorized(ApiResponseService.Failure("Unauthorized to update this team.")); 

            // Remove the user from the database and save changes.
            _context.Users.Remove(user);
            _context.SaveChanges();

            // Return success response after deletion.
            return Ok(ApiResponseService.Success("User deleted successfully."));
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
            // Validate the updated user data.
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseService.Failure("Invalid user data."));
            }

            // Retrieve the current user based on the token.
            var (user, unauthorizedResult) = _tokenService.GetUserFromToken(HttpContext);
            if (unauthorizedResult != null) return Unauthorized(ApiResponseService.Failure("Unauthorized to update this team.")); 

            // Update the user's information.
            user.Username = updatedUser.Username;

            _context.Users.Update(user);
            _context.SaveChanges();

            // Return success response after update.
            return Ok(ApiResponseService.Success("User updated successfully."));
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
            // Retrieve the current user based on the token.
            var (user, unauthorizedResult) = _tokenService.GetUserFromToken(HttpContext);
            if (unauthorizedResult != null) return Unauthorized(ApiResponseService.Failure("Unauthorized to update this team."));

            // Return success response with the user's information.
            return Ok(ApiResponseService.Success("User retrieved successfully.", user));
        }
    }
}
