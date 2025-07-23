using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentQnA.Api.Models
{
    public class ChatHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public string? SourceDocument { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
