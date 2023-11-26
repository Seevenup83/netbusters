using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using netbusters.Data;
using netbusters.Models;
using netbusters.Services;

namespace netbusters.Controllers
{
    [ApiController]
    [Route("api/team")]
    public class TeamController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly TokenService _tokenService;

        public TeamController(DatabaseContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Creates a new team.
        /// </summary>
        [Authorize]
        [HttpPost]
        public IActionResult CreateTeam([FromBody] Team team)
        {
            var (user, unauthorizedResult) = _tokenService.GetUserFromToken(HttpContext);
            if (unauthorizedResult != null) return Unauthorized(ApiResponseService.Failure("Unauthorized to update this team."));

            team.UserId = user.Id;
            _context.Teams.Add(team);
            _context.SaveChanges();

            return Ok(ApiResponseService.Success("Team created successfully", team));
        }

        /// <summary>
        /// Retrieves all teams associated with the logged-in user.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult GetTeams()
        {
            var (user, unauthorizedResult) = _tokenService.GetUserFromToken(HttpContext);
            if (unauthorizedResult != null) return Unauthorized(ApiResponseService.Failure("Unauthorized to update this team."));

            var teams = _context.Teams.Where(t => t.UserId == user.Id).ToList();

            return Ok(ApiResponseService.Success("Teams retrieved successfully", teams));
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
                return NotFound(ApiResponseService.Failure("Team not found"));
            }

            var (user, unauthorizedResult) = _tokenService.GetUserFromToken(HttpContext);
            if (unauthorizedResult != null) return Unauthorized(ApiResponseService.Failure("Unauthorized to update this team."));

            // Check if the authenticated user is the creator of the team
            if (team.UserId != user.Id)
            {
                return Unauthorized(ApiResponseService.Failure("Unauthorized to update this team."));
            }

            // Update team details
            team.Name = updatedTeam.Name;
            // Update other properties as needed

            _context.Teams.Update(team);
            _context.SaveChanges();

            return Ok(ApiResponseService.Success("Team updated successfully", team));
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
                return NotFound(ApiResponseService.Failure("Team not found"));
            }

            var (user, unauthorizedResult) = _tokenService.GetUserFromToken(HttpContext);
            if (unauthorizedResult != null) return Unauthorized(ApiResponseService.Failure("Unauthorized to update this team."));

            // Check if the authenticated user is the creator of the team
            if (team.UserId != user.Id)
            {
                return Unauthorized(ApiResponseService.Failure("Unauthorized to delete this team."));
            }

            _context.Teams.Remove(team);
            _context.SaveChanges();

            return Ok(ApiResponseService.Success("Team deleted successfully"));
        }
    }
}
