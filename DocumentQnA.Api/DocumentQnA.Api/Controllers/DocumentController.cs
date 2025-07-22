// Controllers/DocumentController.cs
using DocumentQnA.Api.Data;
using DocumentQnA.Api.Models;
using DocumentQnA.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using System.Security.Claims; // Required for accessing user claims
using System.IO; // Required for Path, FileStream

namespace DocumentQnA.Api.Controllers
{
    /// <summary>
    /// API controller for managing documents, including viewing, uploading, and deleting.
    /// All actions in this controller require user authentication.
    /// </summary>
    [Authorize] // Requires authentication for all actions in this controller
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ITextExtractor _extractor;
        private readonly AppDbContext _db;
        private readonly IGeminiServices _geminiServices; // NEW: Inject IGeminiServices

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentController"/> class.
        /// </summary>
        /// <param name="env">The web host environment service.</param>
        /// <param name="extractor">The text extraction service.</param>
        /// <param name="db">The application's database context.</param>
        /// <param name="geminiServices">The Gemini AI services for embedding generation.</param> // NEW: Add to docs
        public DocumentController(IWebHostEnvironment env, ITextExtractor extractor, AppDbContext db, IGeminiServices geminiServices) // NEW: Inject IGeminiServices
        {
            _env = env;
            _extractor = extractor;
            _db = db;
            _geminiServices = geminiServices; // NEW: Assign injected service
        }

        /// <summary>
        /// Retrieves a list of all documents.
        /// Requires 'CanViewDocuments' permission or 'Admin' role.
        /// </summary>
        /// <returns>An HTTP 200 OK with a list of documents, or 403 Forbidden if unauthorized.</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDocuments()
        {
            // Check if the user has the 'CanViewDocuments' claim or is an 'Admin'
            var canViewDocuments = HttpContext.User.HasClaim("CanViewDocuments", "True");
            if (!canViewDocuments && !User.IsInRole("Admin"))
            {
                return StatusCode(403, "You do not have permission to view documents.");
            }

            // Retrieve and return all documents
            // Note: We are not returning the RawText or VectorEmbeddingJson directly to the frontend
            // to keep the payload smaller and secure.
            return Ok(await _db.DocumentTexts.Select(d => new { d.Id, d.FileName, d.UploadedAt }).ToListAsync());
        }

        /// <summary>
        /// Uploads a new document to the system.
        /// Requires 'CanUploadDocuments' permission or 'Admin' role.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <returns>An HTTP 200 OK on successful upload, or 400 Bad Request/403 Forbidden/500 Internal Server Error.</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // Check if the user has the 'CanUploadDocuments' claim or is an 'Admin'
            var canUploadDocuments = HttpContext.User.HasClaim("CanUploadDocuments", "True");
            if (!canUploadDocuments && !User.IsInRole("Admin"))
            {
                return StatusCode(403, "You do not have permission to upload documents.");
            }

            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            string tempPath = string.Empty;
            string extractedText = string.Empty;
            double[]? embedding = null; // Initialize embedding

            try
            {
                // Save temporarily for processing
                tempPath = await SaveToTempAsync(file);

                // Extract text from file
                extractedText = await _extractor.ExtractTextAsync(tempPath);

                // NEW: Generate embedding for the extracted text
                if (!string.IsNullOrWhiteSpace(extractedText))
                {
                    embedding = await _geminiServices.GenerateEmbeddingAsync(extractedText);
                }

                // Prepare permanent file path
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var permanentPath = Path.Combine(uploadsFolder, file.FileName);

                // Overwrite only if file is different or allowed
                if (System.IO.File.Exists(permanentPath))
                    System.IO.File.Delete(permanentPath);

                System.IO.File.Copy(tempPath, permanentPath);

                var document = new DocumentText
                {
                    FileName = file.FileName,
                    RawText = extractedText,
                    UploadedAt = DateTime.UtcNow,
                    VectorEmbedding = embedding // NEW: Assign the generated embedding
                };

                _db.DocumentTexts.Add(document);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    Message = "File uploaded, text extracted, and embedding generated successfully.",
                    DocumentId = document.Id,
                    FileName = file.FileName
                });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error during document upload: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, $"Upload failed: {ex.Message}");
            }
            finally
            {
                // Ensure cleanup even if extraction or saving fails
                if (!string.IsNullOrWhiteSpace(tempPath) && System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }

        /// <summary>
        /// Saves an uploaded file to a temporary location.
        /// </summary>
        /// <param name="file">The IFormFile to save.</param>
        /// <returns>The full path to the temporary file.</returns>
        private async Task<string> SaveToTempAsync(IFormFile file)
        {
            var tempPath = Path.Combine(Path.GetTempPath(),
                Guid.NewGuid() + Path.GetExtension(file.FileName));

            using var stream = new FileStream(tempPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return tempPath;
        }

        /// <summary>
        /// Retrieves the text content of a specific document by its ID.
        /// Requires 'CanViewDocuments' permission or 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the document to retrieve.</param>
        /// <returns>An HTTP 200 OK with document details, 404 Not Found, or 403 Forbidden.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentText(int id)
        {
            // Check if the user has the 'CanViewDocuments' claim or is an 'Admin'
            var canViewDocuments = HttpContext.User.HasClaim("CanViewDocuments", "True");
            if (!canViewDocuments && !User.IsInRole("Admin"))
            {
                return StatusCode(403, "You do not have permission to view documents.");
            }

            var doc = await _db.DocumentTexts.FindAsync(id);
            if (doc == null) return NotFound("Document not found.");

            // Do not return RawText or VectorEmbedding to frontend directly for security/performance
            return Ok(new
            {
                doc.Id,
                doc.FileName,
                doc.UploadedAt
                // RawText and VectorEmbedding are intentionally omitted from this public endpoint
            });
        }

        /// <summary>
        /// Deletes a document by its ID.
        /// Requires 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the document to delete.</param>
        /// <returns>An HTTP 204 No Content on successful deletion, 404 Not Found, or 403 Forbidden.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only users with the "Admin" role can delete documents
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _db.DocumentTexts.FindAsync(id);
            if (doc == null) return NotFound();

            _db.DocumentTexts.Remove(doc);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
