using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DocumentQnA.Api.Models
{
    public class DocumentText
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string RawText { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string? VectorEmbeddingJson { get; set; }

        [NotMapped]
        public double[]? VectorEmbedding
        {
            get => VectorEmbeddingJson != null ? JsonSerializer.Deserialize<double[]>(VectorEmbeddingJson) : null;
            set => VectorEmbeddingJson = value != null ? JsonSerializer.Serialize(value) : null;
        }
    }
}
