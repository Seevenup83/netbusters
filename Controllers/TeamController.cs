//Controllers/TeamController.cs
using Microsoft.AspNetCore.Mvc;
using netbusters.Data;
using netbusters.Models;
using netbusters.Common;
using Microsoft.AspNetCore.Authorization;

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
            _context.Teams.Add(team);
            _context.SaveChanges();

            var responseData = new { team.Id, team.Name };
            return Ok(new ApiResponse("Team created successfully", responseData));
        }

        /// <summary>
        /// Get a team.
        /// </summary>  
        [Authorize]
        [HttpGet]
        public IActionResult GetTeams(int clubId)
        {
            // Logic to get all teams in a specific club
            // ...

            var teams = "test";
            return Ok(new ApiResponse("Teams retrieved successfully", teams));
        }

        // Other team-related methods...
    }
}
