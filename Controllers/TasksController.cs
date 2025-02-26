using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicencjatUG.Server.Models;
using LicencjatUG.Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using TaskStatus = LicencjatUG.Server.Models.TaskStatus;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace LicencjatUG.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly DataContext _context;

        public TasksController(DataContext context)
        {
            _context = context;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskEntity>>> GetTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .ToListAsync();
            return Ok(tasks);
        }


        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskEntity>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        [HttpGet("recent-assigned")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TaskEntity>>> GetRecentAssignedTasksForUser()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Brak ID użytkownika w tokenie.");
            }
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Błędny user ID w tokenie.");
            }

            var tasks = await _context.Tasks
                .Where(t => t.AssignedToId == userId)
                .OrderByDescending(t => t.CreatedDate)
                .Take(3)
                .Include(t => t.CreatedBy)     // <-- kluczowe
                .Include(t => t.AssignedTo)    // <-- kluczowe
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TaskEntity>> CreateTask([FromBody] TaskCreateDto taskDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int creatorId))
                return BadRequest("Invalid user ID");

            // Próbujemy sparsować ciąg znaków z taskDto.Status do enum TaskStatus
            if (!Enum.TryParse<TaskStatus>(taskDto.Status, true, out var parsedStatus))
            {
                // Jeśli się nie uda, zwracamy 400 z komunikatem
                return BadRequest("Niepoprawny status zadania. Użyj: 'Issue', 'InProgress' lub 'Done'.");
            }

            var task = new TaskEntity
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Status = parsedStatus,  // przypisujemy sparsowaną wartość enuma
                CreatedById = creatorId,
                AssignedToId = taskDto.AssignedToId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        public class TaskCreateDto
        {
            [Required]
            [StringLength(100)]
            public string Title { get; set; }

            [Required]
            public string Description { get; set; }

            [Required]
            public string Status { get; set; } // "Issue", "InProgress", "Done"

            public int? AssignedToId { get; set; }
        }
        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskEntity task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tasks.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] string newStatus)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound("Zadanie nie zostało znalezione.");

            // Używamy poprawnego TaskStatus
            if (!Enum.TryParse<TaskStatus>(newStatus, true, out var status))
                return BadRequest("Nieprawidłowy status zadania.");

            task.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("tasks-for-user")]
        public async Task<ActionResult<IEnumerable<TaskEntity>>> GetTasksForUser()
        {
            // Pobieramy username z tokena za pomocą ClaimTypes.Name
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Brak username w tokenie.");
            }

            var tasks = await _context.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.CreatedBy.Username == username)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return Ok(tasks);
        }

      
        [HttpGet("my-tasks")]
        [Authorize]
        public async Task<IActionResult> GetMyTasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var tasks = await _context.Tasks
               .Where(t => t.AssignedToId == int.Parse(userId))
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .ToListAsync();

            return Ok(tasks);
        }


    }
}
