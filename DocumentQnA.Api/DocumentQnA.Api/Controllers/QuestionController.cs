using DocumentQnA.Api.Data;
using DocumentQnA.Api.Models;
using DocumentQnA.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentQnA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly GeminiService _gemini;

        public QuestionController(AppDbContext context, GeminiService gemini)
        {
            _context = context;
            _gemini = gemini;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] QuestionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserQuestion))
                return BadRequest("Question cannot be empty.");

            var documents = await _context.DocumentTexts.ToListAsync();

            if (!documents.Any())
                return NotFound("No documents found.");

            string combinedContext = string.Join("\n---\n", documents.Select(d =>
                $"Document: {d.FileName}\n{d.RawText}"
            ));
            string prompt = $"You are answering based on the following documents.\n" +
                "Reference filenames in your response if relevant.\n\n" +
                $"Context:\n{combinedContext}\n\n" +
                $"Question:\n{dto.UserQuestion}";


            string answer = await _gemini.GetAnswerAsync(prompt);

            return Ok(new { answer });
        }

    }
}
