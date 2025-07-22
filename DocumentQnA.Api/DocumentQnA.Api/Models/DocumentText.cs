// Models/DocumentText.cs
using System;
using System.ComponentModel.DataAnnotations.Schema; // Required for [NotMapped]
using System.Collections.Generic; // Required for List<double>
using System.Text.Json; // Required for JsonSerializer

namespace DocumentQnA.Api.Models
{
    /// <summary>
    /// Represents a document's extracted text content and its associated metadata.
    /// Includes a property for storing vector embeddings for similarity search.
    /// </summary>
    public class DocumentText
    {
        /// <summary>
        /// Gets or sets the unique identifier for the document.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the original file name of the document.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or or sets the raw, extracted text content of the document.
        /// </summary>
        public string RawText { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the document was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Stores the vector embedding of the document's content as a JSON string.
        /// This column will be mapped to the database. It is nullable to allow for existing documents
        /// that might not yet have an embedding.
        /// </summary>
        public string? VectorEmbeddingJson { get; set; }

        /// <summary>
        /// Gets or sets the vector embedding of the document's content.
        /// This property is not mapped to the database directly; it's a convenience
        /// property for working with the embedding as a double array in C#.
        /// </summary>
        [NotMapped] // This property will not be mapped to a database column
        public double[]? VectorEmbedding
        {
            get => VectorEmbeddingJson != null ? JsonSerializer.Deserialize<double[]>(VectorEmbeddingJson) : null;
            set => VectorEmbeddingJson = value != null ? JsonSerializer.Serialize(value) : null;
        }
    }
}
