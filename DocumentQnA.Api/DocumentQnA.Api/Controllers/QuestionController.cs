// Controllers/QuestionController.cs
using DocumentQnA.Api.Data;
using DocumentQnA.Api.Models;
using DocumentQnA.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Added for .Select and .Any
using System; // Added for Exception

namespace DocumentQnA.Api.Controllers
{
    /// <summary>
    /// API controller for handling user questions, retrieving relevant documents,
    /// and generating answers using the Gemini AI service.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IGeminiServices _gemini; // Changed to use the interface

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionController"/> class.
        /// </summary>
        /// <param name="context">The application's database context.</param>
        /// <param name="gemini">The Gemini AI service for generating responses (injected as interface).</param>
        public QuestionController(AppDbContext context, IGeminiServices gemini) // Changed constructor parameter
        {
            _context = context;
            _gemini = gemini;
        }

        /// <summary>
        /// Processes a user's question by fetching relevant documents, constructing a prompt,
        /// and getting an answer from the Gemini AI model.
        /// </summary>
        /// <param name="dto">The Data Transfer Object containing the user's question.</param>
        /// <returns>An HTTP 200 OK with the answer and source document, or 400 Bad Request/500 Internal Server Error.</returns>
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] QuestionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserQuestion))
                return BadRequest("Question cannot be empty.");

            var documents = await _context.DocumentTexts.ToListAsync();

            string combinedContext = string.Empty;
            if (documents.Any())
            {
                // Combine documents with filenames, clearly demarcated
                combinedContext = string.Join("\n---END_DOCUMENT---\n", documents.Select(d =>
                    $"Document Name: {d.FileName}\nDocument Content:\n{d.RawText}"
                ));
            }

            // UPDATED SYSTEM INSTRUCTION: Make it much stricter
            string systemInstruction = @"You are a helpful assistant for document search.
Your primary goal is to answer questions *ONLY* based on the 'Document Content:' provided below.
If you cannot find the answer *strictly* within the provided documents, you MUST respond with: 'answer: I cannot find the answer to your question in the provided documents. Please try rephrasing or upload more relevant documents.'
DO NOT use any outside knowledge or general information.
If your answer is directly derived from the content within the 'Document Content:' sections, provide a concise answer. After the answer, include a 'sourceDocument:' field with the exact 'Document Name:' from which the information was found.
If the question is a greeting or a general conversational query (e.g., 'How are you?'), and no documents are provided or relevant, then do NOT include the 'sourceDocument:' field, and provide a general, conversational answer.
Always format your response with 'answer:' on its own line.

Example for document-based answer:
answer: The capital of France is Paris.
sourceDocument: geography_faq.txt

Example for general answer/greeting (only if no documents are provided or question is not document-related):
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
                // Call the Gemini service using the interface and the correct method
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

        /// <summary>
        /// Data Transfer Object (DTO) for incoming questions.
        /// </summary>
        public class QuestionDto
        {
            public string UserQuestion { get; set; }
        }
    }
}
