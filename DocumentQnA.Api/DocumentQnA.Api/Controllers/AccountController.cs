using Microsoft.AspNetCore.Mvc;
using DocumentQnA.Api.Services; 
using Microsoft.AspNetCore.Authorization;

namespace DocumentQnA.Api.Controllers
{
    /// <summary>
    /// API controller for user account management, including registration, login,
    /// and administrative actions like updating user permissions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Defines the base route for this controller (e.g., /api/Account)
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service dependency.</param>
        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration request containing username, password, and optional isAdmin flag.</param>
        /// <returns>An HTTP 200 OK with token on success, or 400 Bad Request on failure.</returns>
        [HttpPost("register")] // Defines the route for registration (e.g., /api/Account/register)
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Call the AuthService to register the user
            var (success, token, message) = await _authService.RegisterUserAsync(request.Username, request.Password, request.IsAdmin);
            if (success)
            {
                // If registration is successful, return the JWT token
                return Ok(new { Token = token, Message = message });
            }
            // If registration fails, return a bad request with the error message
            return BadRequest(new { Message = message });
        }

        /// <summary>
        /// Logs in an existing user account.
        /// </summary>
        /// <param name="request">The login request containing username and password.</param>
        /// <returns>An HTTP 200 OK with token on success, or 401 Unauthorized on failure.</returns>
        [HttpPost("login")] // Defines the route for login (e.g., /api/Account/login)
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Call the AuthService to authenticate the user
            var (success, token, message) = await _authService.LoginAsync(request.Username, request.Password);
            if (success)
            {
                // If login is successful, return the JWT token
                return Ok(new { Token = token, Message = message });
            }
            // If login fails, return an unauthorized response
            return Unauthorized(new { Message = message });
        }

        /// <summary>
        /// Updates the permissions of a specific user. This action requires "Admin" role authorization.
        /// </summary>
        /// <param name="userId">The ID of the user whose permissions are to be updated.</param>
        /// <param name="request">The request containing the new permission values (CanViewDocuments, CanUploadDocuments).</param>
        /// <returns>An HTTP 200 OK on success, or 404 Not Found/400 Bad Request on failure.</returns>
        [Authorize(Roles = "Admin")] // Only users with the "Admin" role can access this endpoint
        [HttpPut("permissions/{userId}")] // Defines the route for updating permissions (e.g., /api/Account/permissions/{userId})
        public async Task<IActionResult> UpdateUserPermissions(string userId, [FromBody] UpdatePermissionsRequest request)
        {
            // Call the AuthService to update the user's permissions
            var success = await _authService.UpdateUserPermissionsAsync(userId, request.CanViewDocuments, request.CanUploadDocuments);
            if (success)
            {
                // If update is successful, return an OK response
                return Ok(new { Message = "User permissions updated successfully." });
            }
            // If update fails (e.g., user not found), return an appropriate error
            return NotFound(new { Message = "User not found or failed to update permissions." });
        }

        /// <summary>
        /// Retrieves a list of all users in the system with their permissions and roles.
        /// This endpoint is restricted to users with the "Admin" role.
        /// </summary>
        /// <returns>An HTTP 200 OK with a list of UserDto objects, or 403 Forbidden if not an Admin.</returns>
        [Authorize(Roles = "Admin")] // Only users with the "Admin" role can access this endpoint
        [HttpGet("users")] // New endpoint for getting all users
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }


        /// <summary>
        /// Data Transfer Object (DTO) for user registration requests.
        /// </summary>
        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            // This flag allows an admin to register another admin.
            // In a real-world scenario, you might want more robust admin creation flows.
            public bool IsAdmin { get; set; } = false;
        }

        /// <summary>
        /// Data Transfer Object (DTO) for user login requests.
        /// </summary>
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        /// <summary>
        /// Data Transfer Object (DTO) for updating user permissions.
        /// </summary>
        public class UpdatePermissionsRequest
        {
            public bool CanViewDocuments { get; set; }
            public bool CanUploadDocuments { get; set; }
        }
    }
}
