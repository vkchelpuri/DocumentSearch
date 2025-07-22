// Controllers/ChatController.cs
using DocumentQnA.Api.Data;
using DocumentQnA.Api.Models;
using DocumentQnA.Api.Services; // Required for IGeminiServices
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using System.Security.Claims; // Required for accessing user claims
using System.Linq; // Required for .Where() and .OrderByDescending()
using System; // Required for Exception
using System.Collections.Generic; // Required for List
using System.Threading.Tasks; // Required for Task

namespace DocumentQnA.Api.Controllers
{
    /// <summary>
    /// API controller for managing chat interactions, including asking questions and retrieving chat history.
    /// All actions in this controller require user authentication.
    /// </summary>
    [Authorize] // Requires authentication for all actions in this controller
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IGeminiServices _geminiServices; // Added for interacting with Gemini service

        // Define a threshold for document relevance (cosine similarity)
        private const double SIMILARITY_THRESHOLD = 0.6; // Adjust as needed
        // Define how many top documents to retrieve for context
        private const int TOP_N_DOCUMENTS = 3; // Adjust as needed

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatController"/> class.
        /// </summary>
        /// <param name="context">The application's database context.</param>
        /// <param name="geminiServices">The Gemini AI service for generating responses and embeddings.</param>
        public ChatController(AppDbContext context, IGeminiServices geminiServices)
        {
            _context = context;
            _geminiServices = geminiServices;
        }

        /// <summary>
        /// Processes a user's question, generates a response using Gemini (with document context),
        /// and saves the interaction to chat history.
        /// </summary>
        /// <param name="questionDto">The DTO containing the user's question.</param>
        /// <returns>An HTTP 200 OK with the generated response, or 401 Unauthorized if user is not identified, or 500 Internal Server Error.</returns>
        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] QuestionDto questionDto)
        {
            // Get the ID of the currently authenticated user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not identified.");
            }

            if (string.IsNullOrWhiteSpace(questionDto?.Query))
            {
                return BadRequest("Question cannot be empty.");
            }

            Console.WriteLine($"--- ChatController.AskQuestion: Processing query: '{questionDto.Query}' ---");

            try
            {
                // 1. Generate embedding for the user's query
                Console.WriteLine("Generating embedding for user query...");
                double[] queryEmbedding = await _geminiServices.GenerateEmbeddingAsync(questionDto.Query);
                Console.WriteLine($"Query Embedding generated. Dimension: {queryEmbedding.Length}");

                // 2. Retrieve all documents with their embeddings from the database
                Console.WriteLine("Retrieving all documents with embeddings from database...");
                var allDocuments = await _context.DocumentTexts
                                                 .Where(d => d.VectorEmbeddingJson != null && d.RawText != null)
                                                 .ToListAsync();
                Console.WriteLine($"Found {allDocuments.Count} documents with embeddings.");

                // 3. Perform semantic search: Calculate similarity and select top N relevant documents
                var relevantDocuments = new List<(DocumentText Document, double Similarity)>();

                if (!allDocuments.Any())
                {
                    Console.WriteLine("No documents with embeddings found in the database. Skipping semantic search.");
                }
                else
                {
                    Console.WriteLine("Calculating cosine similarity for each document...");
                    foreach (var doc in allDocuments)
                    {
                        if (doc.VectorEmbedding != null) // Ensure embedding is not null after deserialization
                        {
                            double similarity = CalculateCosineSimilarity(queryEmbedding, doc.VectorEmbedding);
                            Console.WriteLine($"  Document: '{doc.FileName}', Similarity: {similarity:F4}"); // Format to 4 decimal places
                            if (similarity >= SIMILARITY_THRESHOLD)
                            {
                                relevantDocuments.Add((doc, similarity));
                            }
                        }
                        else
                        {
                            Console.WriteLine($"  WARNING: Document '{doc.FileName}' has null VectorEmbedding despite VectorEmbeddingJson being present. Skipping.");
                        }
                    }
                    Console.WriteLine($"Found {relevantDocuments.Count} documents above similarity threshold ({SIMILARITY_THRESHOLD}).");
                }


                // Order by similarity and take the top N
                var topNDocuments = relevantDocuments
                                    .OrderByDescending(d => d.Similarity)
                                    .Take(TOP_N_DOCUMENTS)
                                    .ToList();

                string combinedContext = string.Empty;
                string sourceDocumentNames = "None"; // Default source if no documents are found or relevant

                if (topNDocuments.Any())
                {
                    Console.WriteLine($"Selected Top {topNDocuments.Count} relevant documents:");
                    // Combine raw text from the top N relevant documents
                    combinedContext = string.Join("\n---END_DOCUMENT---\n", topNDocuments.Select(d =>
                    {
                        Console.WriteLine($"  - '{d.Document.FileName}' (Similarity: {d.Similarity:F4})");
                        return $"Document Name: {d.Document.FileName}\nDocument Content:\n{d.Document.RawText}";
                    }));

                    // Collect source document names for the response
                    sourceDocumentNames = string.Join(", ", topNDocuments.Select(d => d.Document.FileName));
                }
                else
                {
                    Console.WriteLine("No relevant documents found above the threshold. Combined context will be empty.");
                }

                // Strict system instruction for Gemini
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
                string fullPrompt = systemInstruction;

                if (!string.IsNullOrWhiteSpace(combinedContext))
                {
                    fullPrompt += "\n" + combinedContext + "\n---END_OF_ALL_DOCUMENTS---\n\n";
                }
                else
                {
                    fullPrompt += "\nNo specific documents are provided for this query.\n\n";
                }

                fullPrompt += $"User Question: {questionDto.Query}";

                Console.WriteLine("--- Full Prompt sent to Gemini (truncated for brevity): ---");
                Console.WriteLine(fullPrompt.Length > 500 ? fullPrompt.Substring(0, 500) + "..." : fullPrompt);
                Console.WriteLine("----------------------------------------------------------");


                // Call your Gemini service with the full prompt including document context
                var (geminiAnswer, _) = await _geminiServices.GetAnswerWithDocumentAsync(fullPrompt); // We now generate source based on our search

                Console.WriteLine($"Gemini Answer Received: {geminiAnswer}");
                Console.WriteLine($"Source Document Names (from semantic search): {sourceDocumentNames}");

                // Create a new ChatHistory entry
                var chatHistory = new ChatHistory
                {
                    UserId = userId,
                    Question = questionDto.Query,
                    Answer = geminiAnswer,
                    SourceDocument = sourceDocumentNames, // Use the names of the top N documents as source
                    Timestamp = DateTime.UtcNow
                };

                // Add the chat history entry to the database
                _context.ChatHistories.Add(chatHistory);
                await _context.SaveChangesAsync();

                Console.WriteLine("--- ChatController.AskQuestion: Query processing complete. ---");
                return Ok(new { Response = geminiAnswer, SourceDocument = sourceDocumentNames });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error in AskQuestion: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, $"Error processing chat: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the cosine similarity between two vectors.
        /// Cosine similarity measures the cosine of the angle between two vectors.
        /// A value of 1 means the vectors are identical, 0 means they are orthogonal (unrelated),
        /// and -1 means they are diametrically opposed.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The cosine similarity between the two vectors.</returns>
        /// <exception cref="ArgumentException">Thrown if vectors have different lengths or are empty.</exception>
        private double CalculateCosineSimilarity(double[] vector1, double[] vector2)
        {
            if (vector1 == null || vector2 == null || vector1.Length == 0 || vector2.Length == 0)
            {
                throw new ArgumentException("Vectors cannot be null or empty.");
            }
            if (vector1.Length != vector2.Length)
            {
                throw new ArgumentException("Vectors must have the same length.");
            }

            double dotProduct = 0.0;
            double magnitude1 = 0.0;
            double magnitude2 = 0.0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
            {
                return 0.0; // Avoid division by zero, vectors are zero vectors
            }

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Retrieves the chat history for the currently authenticated user.
        /// </summary>
        /// <returns>An HTTP 200 OK with a list of chat history entries, or 401 Unauthorized if user is not identified, or 500 Internal Server Error.</returns>
        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory()
        {
            // Get the ID of the currently authenticated user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not identified.");
            }

            try
            {
                // Retrieve chat history for the specific user, ordered by timestamp
                var history = await _context.ChatHistories
                                            .Where(h => h.UserId == userId)
                                            .OrderBy(h => h.Timestamp)
                                            .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error in GetChatHistory: {ex.Message}");
                return StatusCode(500, $"Error loading chat history: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the entire chat history. This action is restricted to 'Admin' role.
        /// </summary>
        /// <returns>An HTTP 200 OK on success, or 403 Forbidden if not an Admin, or 500 Internal Server Error.</returns>
        [HttpDelete("clear")]
        [Authorize(Roles = "Admin")] // Only users with the "Admin" role can clear chat history
        public async Task<IActionResult> ClearChat()
        {
            try
            {
                // Retrieve all chat history entries and remove them
                _context.ChatHistories.RemoveRange(_context.ChatHistories);
                await _context.SaveChangesAsync();

                return Ok("Chat history cleared.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ClearChat: {ex.Message}");
                return StatusCode(500, $"Error clearing chat: {ex.Message}");
            }
        }

        /// <summary>
        /// Data Transfer Object (DTO) for incoming chat questions.
        /// </summary>
        public class QuestionDto
        {
            public string Query { get; set; }
        }
    }
}
