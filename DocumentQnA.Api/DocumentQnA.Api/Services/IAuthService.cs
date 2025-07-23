using DocumentQnA.Api.Models; 

namespace DocumentQnA.Api.Services
{
    /// <summary>
    /// Defines the contract for authentication and authorization operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the specified username and password,
        /// optionally assigning them as an admin.
        /// </summary>
        Task<(bool Success, string Token, string Message)> RegisterUserAsync(string username, string password, bool isAdmin = false);

        /// <summary>
        /// Authenticates a user with the provided username and password.
        /// </summary>
        Task<(bool Success, string Token, string Message)> LoginAsync(string username, string password);

        /// <summary>
        /// Retrieves an ApplicationUser by their unique ID.
        /// </summary>
        Task<ApplicationUser> GetUserByIdAsync(string userId);

        /// <summary>
        /// Retrieves an ApplicationUser by their username.
        /// </summary>
        Task<ApplicationUser> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Updates the document viewing and uploading permissions for a specific user.
        /// This operation is typically restricted to administrators.
        /// </summary>
        Task<bool> UpdateUserPermissionsAsync(string userId, bool canViewDocuments, bool canUploadDocuments);

        /// <summary>
        /// Creates an initial admin account if one does not already exist.
        /// This is useful for bootstrapping the application with a default administrator.
        /// </summary>
        Task<bool> CreateAdminAccountIfNotExist();

        /// <summary>
        /// Assigns a specified role to a user.
        /// </summary>
        Task<bool> AssignRoleToUserAsync(ApplicationUser user, string roleName);

        /// <summary>
        /// Retrieves a list of all application users with their permissions and roles.
        /// </summary>
        /// <returns>A list of UserDto objects containing user details, permissions, and roles.</returns>
        Task<List<UserDto>> GetAllUsersAsync();
    }

    /// <summary>
    /// Data Transfer Object (DTO) for returning user information, including permissions and roles.
    /// </summary>
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public bool CanViewDocuments { get; set; }
        public bool CanUploadDocuments { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
