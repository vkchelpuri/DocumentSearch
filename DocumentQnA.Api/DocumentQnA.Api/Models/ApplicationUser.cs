using Microsoft.AspNetCore.Identity;

namespace DocumentQnA.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool CanViewDocuments { get; set; } = true;
        public bool CanUploadDocuments { get; set; } = false;
        public ICollection<ChatHistory> ChatHistories { get; set; }
    }
}
