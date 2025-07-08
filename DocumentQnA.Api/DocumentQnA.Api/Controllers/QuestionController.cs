using DocumentQnA.Api.Data;
using DocumentQnA.Api.Models;
using DocumentQnA.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Added for .Select and .Any

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
            // It's generally good practice to not proceed if there are no documents,
            // as the Gemini model wouldn't have any context to draw from for document-based answers.
            // However, for greetings, it can still function. We'll handle this dynamically.

            string combinedContext = string.Empty;
            if (documents.Any())
            {
                // Combine documents with filenames, clearly demarcated
                combinedContext = string.Join("\n---END_DOCUMENT---\n", documents.Select(d =>
                    $"Document Name: {d.FileName}\nDocument Content:\n{d.RawText}"
                ));
            }

            // System instruction to guide Gemini's behavior
            // This is part of the prompt sent to Gemini, defining its role and output format.
            string systemInstruction = @"You are a helpful assistant for document search.
When answering questions, prioritize the provided documents.
If your answer is directly derived from the content within the 'Document Content:' sections, provide a concise answer. After the answer, include a 'sourceDocument:' field with the exact 'Document Name:' from which the information was found.
If the question is a greeting, a general conversational query (e.g., 'How are you?'), or if the answer is NOT directly found or derivable from any of the provided documents, then do NOT include the 'sourceDocument:' field. In such cases, provide a general, conversational answer.
Always format your response with 'answer:' on its own line.

Example for document-based answer:
answer: The capital of France is Paris.
sourceDocument: geography_faq.txt

Example for general answer/greeting:
answer: Hello! How can I assist you today?

Here are the documents for your reference:
";

            // Construct the full prompt to send to Gemini
            // The order is important: System Instruction -> Documents -> User Question
            string fullPrompt = systemInstruction;

            if (!string.IsNullOrWhiteSpace(combinedContext))
            {
                fullPrompt += "\n" + combinedContext + "\n---END_OF_ALL_DOCUMENTS---\n\n";
            }
            else
            {
                // If no documents are present, inform Gemini.
                fullPrompt += "\nNo specific documents are provided for this query.\n\n";
            }

            fullPrompt += $"User Question: {dto.UserQuestion}";

            try
            {
                var (answer, sourceDocument) = await _gemini.GetAnswerWithDocumentAsync(fullPrompt);

                return Ok(new
                {
                    answer = answer ?? "No response",
                    sourceDocument = sourceDocument // This will already be "None" if not found by GeminiService
                });
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                Console.WriteLine($"Error in QuestionController: {ex.Message}");
                return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
            }
        }
    }
}