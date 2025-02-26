using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicencjatUG.Server.Data;
using LicencjatUG.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LicencjatUG.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly DataContext _context;

        public ChatController(DataContext context)
        {
            _context = context;
        }

        // GET: api/chat/{user1}/{user2}
        [HttpGet("{user1}/{user2}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(string user1, string user2)
        {
            return await _context.Messages
                .Where(m => (m.Sender == user1 && m.Receiver == user2)
                         || (m.Sender == user2 && m.Receiver == user1))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Message>> SendMessage(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(message);
        }

    }
}
