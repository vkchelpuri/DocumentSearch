using DocumentQnA.Api.Data;
using DocumentQnA.Api.Models;
using DocumentQnA.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentQnA.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ITextExtractor _extractor;
        private readonly AppDbContext _db;

        public DocumentController(IWebHostEnvironment env, ITextExtractor extractor, AppDbContext db)
        {
            _env = env;
            _extractor = extractor;
            _db = db;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDocuments() =>
            Ok(await _db.DocumentTexts.ToListAsync());

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            string tempPath = string.Empty;
            string extractedText = string.Empty;

            try
            {
                // Save temporarily for processing
                tempPath = await SaveToTempAsync(file);

                // Extract text from file
                extractedText = await _extractor.ExtractTextAsync(tempPath);

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
                    UploadedAt = DateTime.UtcNow
                };

                _db.DocumentTexts.Add(document);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    Message = "File uploaded and text extracted successfully.",
                    DocumentId = document.Id,
                    FileName = file.FileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Upload failed: {ex.Message}");
            }
            finally
            {
                // Ensure cleanup even if extraction or saving fails
                if (!string.IsNullOrWhiteSpace(tempPath) && System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }


        private async Task<string> SaveToTempAsync(IFormFile file)
        {
            var tempPath = Path.Combine(Path.GetTempPath(),
                Guid.NewGuid() + Path.GetExtension(file.FileName));

            using var stream = new FileStream(tempPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return tempPath;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentText(int id)
        {
            var doc = await _db.DocumentTexts.FindAsync(id);
            if (doc == null) return NotFound("Document not found.");

            return Ok(new
            {
                doc.Id,
                doc.FileName,
                doc.RawText,
                doc.UploadedAt
            });
        }

        [HttpDelete("{id}")]
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
