using Microsoft.AspNetCore.Identity;

namespace DocumentQnA.Api.Models
{
    /// <summary>
    /// Represents a user in the application, extending the default IdentityUser
    /// to include custom properties for permissions and navigation to chat history.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Indicates whether the user has permission to view documents.
        /// Defaults to true for new users.
        /// </summary>
        public bool CanViewDocuments { get; set; } = true;

        /// <summary>
        /// Indicates whether the user has permission to upload documents.
        /// Defaults to false for new users. This permission is typically managed by an admin.
        /// </summary>
        public bool CanUploadDocuments { get; set; } = false;

        /// <summary>
        /// Navigation property to represent the collection of chat histories
        /// associated with this user.
        /// </summary>
        public ICollection<ChatHistory> ChatHistories { get; set; }
    }
}
