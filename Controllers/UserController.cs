using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicencjatUG.Server.Models;
using LicencjatUG.Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace LicencjatUG.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;

        public UsersController(DataContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdString, out int currentUserId))
                return Unauthorized("Invalid user ID in token.");

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser == null)
                return NotFound("User not found.");

            return Ok(currentUser);
        }
    }
}
