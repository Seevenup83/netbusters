//Controllers/ClubController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using netbusters.Data;
using netbusters.Models;
using netbusters.Common;
using Microsoft.AspNetCore.Authorization;

namespace netbusters.Controllers
{   
    [ApiController]
    [Route("api/club")]
    public class ClubController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public ClubController(DatabaseContext context)
        {
            _context = context;
        }

        // Helper method to get user ID from token
        private int GetUserIdFromToken()
        {
            if (int.TryParse(User.FindFirst("nameid")?.Value, out var userId))
            {
                return userId;
            }
            else
            {
                // Directly return a response to terminate the flow in the calling method
                Response.StatusCode = 401; // Unauthorized
                Response.ContentType = "application/json";
                var unauthorizedResponse = new ApiResponse("User ID not found in token.", null);
                Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(unauthorizedResponse));
                return 0; // The return value will not be used since the response is already set
            }
        }

        /// <summary>
        /// Creates a new club for the logged-in user.
        /// </summary>  
        [Authorize]
        [HttpPost]
        public IActionResult CreateClub([FromBody] Club club)
        {
            var userId = GetUserIdFromToken();
            if (Response.HasStarted) return new EmptyResult();

            club.UserId = userId;
            _context.Clubs.Add(club);
            _context.SaveChanges();
            return Ok(new ApiResponse("Club created successfully", new { club.Id, club.Name, club.HttpLink }));
        }

        /// <summary>
        /// Retrieves all clubs owned by the logged-in user.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult GetClubs()
        {
            var userId = GetUserIdFromToken();
            if (Response.HasStarted) return new EmptyResult();

            var clubs = _context.Clubs.Where(c => c.UserId == userId).ToList();
            return Ok(new ApiResponse("Clubs retrieved successfully", clubs));
        }

        /// <summary>
        /// Updates a club owned by the logged-in user.
        /// </summary>
        [Authorize]
        [HttpPut("{clubId}")]
        public IActionResult UpdateClub(int clubId, string name, string httpLink = null)
        {
            var userId = GetUserIdFromToken();
            if (Response.HasStarted) return new EmptyResult();

            var club = _context.Clubs.FirstOrDefault(c => c.Id == clubId && c.UserId == userId);
            if (club == null)
            {
                return NotFound(new ApiResponse("Club not found.", null));
            }

            club.Name = name;
            club.HttpLink = httpLink;
            _context.SaveChanges();
            return Ok(new ApiResponse("Club updated successfully", new { club.Id, club.Name, club.HttpLink }));
        }

        /// <summary>
        /// Deletes a club owned by the logged-in user.
        /// </summary>
        [Authorize]
        [HttpDelete("{clubId}")]
        public IActionResult DeleteClub(int clubId)
        {
            var userId = GetUserIdFromToken();
            if (Response.HasStarted) return new EmptyResult();

            var club = _context.Clubs.FirstOrDefault(c => c.Id == clubId && c.UserId == userId);
            if (club == null)
            {
                return NotFound(new ApiResponse("Club not found.", null));
            }

            _context.Clubs.Remove(club);
            _context.SaveChanges();
            return Ok(new ApiResponse("Club deleted successfully", null));
        }
    }
}