using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentQnA.Api.Models
{
    public class ChatHistory
    {
        /// <summary>
        /// Gets or sets the unique identifier for the chat history entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who initiated this chat entry.
        /// This is a foreign key to the ApplicationUser table.
        /// </summary>
        public string UserId { get; set; } // Foreign key to ApplicationUser

        /// <summary>
        /// Navigation property to the ApplicationUser who owns this chat history entry.
        /// </summary>
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // Navigation property

        /// <summary>
        /// Gets or sets the question asked by the user.
        /// </summary>
        public string? Question { get; set; }

        /// <summary>
        /// Gets or sets the answer provided by the system.
        /// </summary>
        public string? Answer { get; set; }

        /// <summary>
        /// Gets or sets the source document from which the answer was derived, if applicable.
        /// </summary>
        public string? SourceDocument { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this chat entry was created.
        /// Defaults to the current UTC time.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
