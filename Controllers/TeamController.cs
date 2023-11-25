//Controllers/TeamController.cs
using Microsoft.AspNetCore.Mvc;
using netbusters.Data;
using netbusters.Models;
using netbusters.Common;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace netbusters.Controllers
{
    [ApiController]
    [Route("api/team")]
    public class TeamController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public TeamController(DatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new team.
        /// </summary>
        [Authorize]
        [HttpPost]
        public IActionResult CreateTeam([FromBody] Team team)
        {
            // Assuming you extract the UserId from the token
            var userId = 1/* Extract UserId from token */;
            team.UserId = userId;

            _context.Teams.Add(team);
            _context.SaveChanges();

            return Ok(ApiResponse.Success("Team created successfully", team));
        }

        /// <summary>
        /// Retrieves all teams associated with the logged-in user.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult GetTeams()
        {
            var userId = 1 /* Extract UserId from token */;
            var teams = _context.Teams.Where(t => t.UserId == userId).ToList();

            return Ok(ApiResponse.Success("Teams retrieved successfully", teams));
        }

        /// <summary>
        /// Updates a specific team.
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateTeam(int id, [FromBody] Team updatedTeam)
        {
            var team = _context.Teams.Find(id);
            if (team == null)
            {
                return NotFound(ApiResponse.Failure("Team not found"));
            }

            // Update team details
            team.Name = updatedTeam.Name;
            // Update other properties as needed

            _context.Teams.Update(team);
            _context.SaveChanges();

            return Ok(ApiResponse.Success("Team updated successfully", team));
        }

        /// <summary>
        /// Deletes a specific team.
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteTeam(int id)
        {
            var team = _context.Teams.Find(id);
            if (team == null)
            {
                return NotFound(ApiResponse.Failure("Team not found"));
            }

            _context.Teams.Remove(team);
            _context.SaveChanges();

            return Ok(ApiResponse.Success("Team deleted successfully"));
        }
    }
}
