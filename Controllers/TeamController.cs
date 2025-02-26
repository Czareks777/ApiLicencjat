using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicencjatUG.Server.Models;
using LicencjatUG.Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LicencjatUG.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly DataContext _context;

        public TeamController(DataContext context)
        {
            _context = context;
        }

        // GET: api/team
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetAllTeams()
        {
            try
            {
                var teams = await _context.Teams.ToListAsync();
                return Ok(teams);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error in GetAllTeams: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/team/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetTeam(int id)
        {
            try
            {
                var team = await _context.Teams
                    .Include(t => t.Owner)
                    .FirstOrDefaultAsync(t => t.Id == id);
                if (team == null)
                    return NotFound();
                return Ok(team);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error in GetTeam: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/team/create
        // Tworzy nowy zespół – właścicielem zostaje zalogowany użytkownik
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<Team>> CreateTeam([FromBody] Team team)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdString, out int ownerId))
                    return BadRequest("Invalid user ID");

                // Ustawiamy właściciela zespołu na zalogowanego użytkownika
                team.OwnerId = ownerId;
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error in CreateTeam: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/team/{teamId}/members
        // Pobiera wszystkich członków danego zespołu
        [HttpGet("{teamId}/members")]
        public async Task<ActionResult<IEnumerable<User>>> GetTeamMembers(int teamId)
        {
            try
            {
                var members = await _context.Users
                    .Where(u => u.TeamId == teamId)
                    .ToListAsync();
                return Ok(members);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error in GetTeamMembers: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/team/delete/{teamId}
        // Usuwa zespół – tylko właściciel zespołu może to zrobić
        [HttpDelete("delete/{teamId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdString, out int userId))
                    return BadRequest("Invalid user ID");

                var team = await _context.Teams.FindAsync(teamId);
                if (team == null)
                    return NotFound();

                if (team.OwnerId != userId)
                    return Forbid("Only the team owner can delete the team.");

                // Ustawiamy TeamId na null u członków zespołu
                var members = await _context.Users.Where(u => u.TeamId == teamId).ToListAsync();
                foreach (var member in members)
                {
                    member.TeamId = null;
                }

                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error in DeleteTeam: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DTO do dodawania członka zespołu
        public class AddMemberDto
        {
            [Required]
            [JsonPropertyName("userId")]
            public int UserId { get; set; }       // ID użytkownika, którego chcemy dodać

            [Required]
            public string Position { get; set; }    // Stanowisko, jakie ma pełnić w zespole
        }

        // POST: api/team/add-member
        // Dodaje użytkownika do zespołu – użytkownik zostanie przypisany do zespołu, którego właścicielem jest zalogowany użytkownik
        [HttpPost("add-member")]
        [Authorize]
        public async Task<IActionResult> AddMember([FromBody] AddMemberDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Pobieramy ID zalogowanego użytkownika z tokena
                var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(currentUserIdString, out int currentUserId))
                    return Unauthorized("Invalid user ID in token.");

                // Sprawdzamy, czy użytkownik w ogóle istnieje
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    return BadRequest("Current user does not exist in the database.");

                // Sprawdzamy, czy jest w zespole
                if (!currentUser.TeamId.HasValue)
                    return BadRequest("You are not in any team. Join or create a team first.");

                // Znajdujemy użytkownika, którego chcemy dodać
                var userToAdd = await _context.Users.FindAsync(dto.UserId);
                if (userToAdd == null)
                    return BadRequest($"User with ID {dto.UserId} does not exist.");

                // Sprawdzamy, czy ten użytkownik nie jest już w innym zespole
                if (userToAdd.TeamId.HasValue && userToAdd.TeamId.Value != 0)
                    return BadRequest("This user is already a member of a team.");

                // Przypisujemy go do zespołu zalogowanego użytkownika
                userToAdd.TeamId = currentUser.TeamId;
                userToAdd.Position = dto.Position;

                await _context.SaveChangesAsync();

                return Ok($"User {userToAdd.Name} has been added to your team (ID {currentUser.TeamId}).");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error in AddMember: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/team/remove-member/{userId}
        // Usuwa użytkownika z zespołu (przypisuje TeamId na null)
        [HttpPost("remove-member/{userId}")]
        [Authorize]
        public async Task<IActionResult> RemoveMember(int userId)
        {
            try
            {
                // Identyfikacja zalogowanego użytkownika
                var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(currentUserIdString, out int currentUserId))
                    return Unauthorized("Invalid user ID in token.");

                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    return BadRequest("Current user does not exist in the database.");

                if (!currentUser.TeamId.HasValue)
                    return BadRequest("You are not in any team. Join or create a team first.");

                // Znajdujemy użytkownika, którego chcemy usunąć
                var userToRemove = await _context.Users.FindAsync(userId);
                if (userToRemove == null)
                    return NotFound("User not found.");

                // Sprawdzamy, czy oboje są w tym samym zespole
                if (userToRemove.TeamId != currentUser.TeamId)
                {
                    return BadRequest("This user is not in your team.");
                }

                // Usunięcie z zespołu (TeamId na null)
                userToRemove.TeamId = null;
                await _context.SaveChangesAsync();

                return Ok($"User {userToRemove.Name} has been removed from the team (ID {currentUser.TeamId}).");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error in RemoveMember: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
