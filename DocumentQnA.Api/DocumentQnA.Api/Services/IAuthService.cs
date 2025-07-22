// Services/IAuthService.cs
using DocumentQnA.Api.Models; // Required for ApplicationUser
using System.Collections.Generic; // Required for List
using System.Threading.Tasks;

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
        /// <param name="username">The desired username for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="isAdmin">A flag indicating if the user should be registered as an admin.</param>
        /// <returns>A tuple indicating success, the JWT token, and a message.</returns>
        Task<(bool Success, string Token, string Message)> RegisterUserAsync(string username, string password, bool isAdmin = false);

        /// <summary>
        /// Authenticates a user with the provided username and password.
        /// </summary>
        /// <param name="username">The username of the user attempting to log in.</param>
        /// <param name="password">The password of the user attempting to log in.</param>
        /// <returns>A tuple indicating success, the JWT token, and a message.</returns>
        Task<(bool Success, string Token, string Message)> LoginAsync(string username, string password);

        /// <summary>
        /// Retrieves an ApplicationUser by their unique ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The ApplicationUser object if found, otherwise null.</returns>
        Task<ApplicationUser> GetUserByIdAsync(string userId);

        /// <summary>
        /// Retrieves an ApplicationUser by their username.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>The ApplicationUser object if found, otherwise null.</returns>
        Task<ApplicationUser> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Updates the document viewing and uploading permissions for a specific user.
        /// This operation is typically restricted to administrators.
        /// </summary>
        /// <param name="userId">The ID of the user whose permissions are to be updated.</param>
        /// <param name="canViewDocuments">The new value for CanViewDocuments permission.</param>
        /// <param name="canUploadDocuments">The new value for CanUploadDocuments permission.</param>
        /// <returns>True if the permissions were updated successfully, false otherwise.</returns>
        Task<bool> UpdateUserPermissionsAsync(string userId, bool canViewDocuments, bool canUploadDocuments);

        /// <summary>
        /// Creates an initial admin account if one does not already exist.
        /// This is useful for bootstrapping the application with a default administrator.
        /// </summary>
        /// <returns>True if an admin account was created, false if it already existed or creation failed.</returns>
        Task<bool> CreateAdminAccountIfNotExist();

        /// <summary>
        /// Assigns a specified role to a user.
        /// </summary>
        /// <param name="user">The ApplicationUser to whom the role will be assigned.</param>
        /// <param name="roleName">The name of the role to assign (e.g., "Admin", "User").</param>
        /// <returns>True if the role was assigned successfully, false otherwise.</returns>
        Task<bool> AssignRoleToUserAsync(ApplicationUser user, string roleName);

        /// <summary>
        /// Retrieves a list of all application users with their permissions and roles.
        /// </summary>
        /// <returns>A list of UserDto objects containing user details, permissions, and roles.</returns>
        Task<List<UserDto>> GetAllUsersAsync(); // NEW METHOD
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
