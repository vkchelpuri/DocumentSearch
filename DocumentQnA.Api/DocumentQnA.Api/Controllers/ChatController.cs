using DocumentQnA.Api.Data;
using DocumentQnA.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentQnA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChatController(AppDbContext context) => _context = context;

        [HttpPost("save")]
        public async Task<IActionResult> SaveChat([FromBody] List<ChatHistory> messages)
        {
            foreach (var msg in messages)
            {
                msg.Timestamp = DateTime.UtcNow;
            }

            _context.ChatHistories.AddRange(messages);
            await _context.SaveChangesAsync();
            return Ok("Chat saved.");
        }

        [HttpGet("load")]
        public async Task<IActionResult> LoadChat()
        {
            var history = await _context.ChatHistories
                .OrderBy(h => h.Timestamp)
                .ToListAsync();
            return Ok(history);
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearChat()
        {
            _context.ChatHistories.RemoveRange(_context.ChatHistories);
            await _context.SaveChangesAsync();
            return Ok("Chat cleared.");
        }
    }


}
