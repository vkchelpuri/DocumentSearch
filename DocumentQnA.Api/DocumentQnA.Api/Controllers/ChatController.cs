using DocumentQnA.Api.Data;
using DocumentQnA.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentQnA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveChat([FromBody] List<ChatHistory> messages)
        {
            if (messages == null || !messages.Any())
                return BadRequest("No messages received.");

            try
            {
                foreach (var msg in messages)
                {
                    msg.Timestamp = DateTime.UtcNow;
                }

                await _context.ChatHistories.AddRangeAsync(messages);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Chat saved successfully.",
                    Count = messages.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving chat: {ex.Message}");
            }
        }

        [HttpGet("load")]
        public async Task<IActionResult> LoadChat()
        {
            try
            {
                var history = await _context.ChatHistories
                    .OrderBy(h => h.Timestamp)
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error loading chat: {ex.Message}");
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearChat()
        {
            try
            {
                _context.ChatHistories.RemoveRange(_context.ChatHistories);
                await _context.SaveChangesAsync();

                return Ok("Chat history cleared.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error clearing chat: {ex.Message}");
            }
        }
    }
}
