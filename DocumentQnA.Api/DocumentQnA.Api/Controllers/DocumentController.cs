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
        public async Task<IActionResult> GetAllDocuments() => Ok(await _db.DocumentTexts.ToListAsync());

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string extractedText = await _extractor.ExtractTextAsync(filePath);

            var document = new DocumentText
            {
                FileName = file.FileName,
                RawText = extractedText,
                UploadedAt = DateTime.UtcNow
            };

            _db.Documents.Add(document);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Message = "File uploaded and text extracted successfully.",
                DocumentId = document.Id,
                FileName = file.FileName
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentText(int id)
        {
            var doc = await _db.Documents.FindAsync(id);
            if (doc == null) return NotFound("Document not found.");

            return Ok(new
            {
                doc.Id,
                doc.FileName,
                doc.RawText,
                doc.UploadedAt
            });
        }
    }
}
