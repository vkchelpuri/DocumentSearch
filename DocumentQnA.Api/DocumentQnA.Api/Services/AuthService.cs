// Services/AuthService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration; // Required for IConfiguration
using Microsoft.IdentityModel.Tokens; // Required for SymmetricSecurityKey, SigningCredentials
using System.IdentityModel.Tokens.Jwt; // Required for JwtSecurityToken, JwtSecurityTokenHandler
using System.Security.Claims; // Required for Claim, ClaimTypes
using System.Text; // Required for Encoding
using DocumentQnA.Api.Models; // Required for ApplicationUser
using System.Threading.Tasks;
using System;
using System.Linq; // Required for Select in error messages
using System.Collections.Generic; // Required for List

namespace DocumentQnA.Api.Services
{
    /// <summary>
    /// Provides services for user authentication and authorization, including
    /// registration, login, JWT token generation, and user permission management.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager; // Added for role management
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="userManager">The UserManager for managing user accounts.</param>
        /// <param name="signInManager">The SignInManager for handling user sign-in.</param>
        /// <param name="roleManager">The RoleManager for managing roles.</param>
        /// <param name="configuration">The application's configuration for JWT settings.</param>
        public AuthService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           RoleManager<IdentityRole> roleManager,
                           IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user with the specified username and password,
        /// optionally assigning them as an admin.
        /// </summary>
        /// <param name="username">The desired username for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="isAdmin">A flag indicating if the user should be registered as an admin.</param>
        /// <returns>A tuple indicating success, the JWT token, and a message.</returns>
        public async Task<(bool Success, string Token, string Message)> RegisterUserAsync(string username, string password, bool isAdmin = false)
        {
            // Create a new ApplicationUser instance
            var user = new ApplicationUser { UserName = username, Email = $"{username}@example.com" }; // Consider taking email as input

            // Attempt to create the user with the provided password
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // If user creation is successful, assign roles based on isAdmin flag
                if (isAdmin)
                {
                    // Ensure the "Admin" role exists, then assign it to the user
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }
                    await _userManager.AddToRoleAsync(user, "Admin");
                    // Set default admin permissions
                    user.CanViewDocuments = true;
                    user.CanUploadDocuments = true;
                    await _userManager.UpdateAsync(user); // Save permission changes
                }
                else
                {
                    // Ensure the "User" role exists, then assign it to the user
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }
                    await _userManager.AddToRoleAsync(user, "User");
                }

                // Generate JWT token for the newly registered user
                var token = GenerateJwtToken(user);
                return (true, token, "Registration successful.");
            }

            // If user creation failed, return error messages
            return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        /// <summary>
        /// Authenticates a user with the provided username and password.
        /// </summary>
        /// <param name="username">The username of the user attempting to log in.</param>
        /// <param name="password">The password of the user attempting to log in.</param>
        /// <returns>A tuple indicating success, the JWT token, and a message.</returns>
        public async Task<(bool Success, string Token, string Message)> LoginAsync(string username, string password)
        {
            // Find the user by username
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return (false, null, "Invalid credentials.");
            }

            // Check if the provided password is correct
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false); // false for lockoutOnFailure
            if (result.Succeeded)
            {
                // Generate JWT token upon successful login
                var token = GenerateJwtToken(user);
                return (true, token, "Login successful.");
            }

            return (false, null, "Invalid credentials.");
        }

        /// <summary>
        /// Retrieves an ApplicationUser by their unique ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The ApplicationUser object if found, otherwise null.</returns>
        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        /// <summary>
        /// Retrieves an ApplicationUser by their username.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>The ApplicationUser object if found, otherwise null.</returns>
        public async Task<ApplicationUser> GetUserByUsernameAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        /// <summary>
        /// Updates the document viewing and uploading permissions for a specific user.
        /// This operation is typically restricted to administrators.
        /// </summary>
        /// <param name="userId">The ID of the user whose permissions are to be updated.</param>
        /// <param name="canViewDocuments">The new value for CanViewDocuments permission.</param>
        /// <param name="canUploadDocuments">The new value for CanUploadDocuments permission.</param>
        /// <returns>True if the permissions were updated successfully, false otherwise.</returns>
        public async Task<bool> UpdateUserPermissionsAsync(string userId, bool canViewDocuments, bool canUploadDocuments)
        {
            // Find the user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false; // User not found
            }

            // Update the permission properties
            user.CanViewDocuments = canViewDocuments;
            user.CanUploadDocuments = canUploadDocuments;

            // Save the changes to the user in the database
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        /// <summary>
        /// Creates an initial admin account if one does not already exist.
        /// This is useful for bootstrapping the application with a default administrator.
        /// </summary>
        /// <returns>True if an admin account was created, false if it already existed or creation failed.</returns>
        public async Task<bool> CreateAdminAccountIfNotExist()
        {
            // Check if the "Admin" role exists, create it if not
            var adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
            if (!adminRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Check if a user with the username "admin" already exists
            var adminUser = await _userManager.FindByNameAsync("admin"); // Default admin username
            if (adminUser == null)
            {
                // If not, create a new admin user
                var newAdmin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@example.com", // Placeholder email
                    CanViewDocuments = true,
                    CanUploadDocuments = true
                };
                // Create the user with a default password
                var result = await _userManager.CreateAsync(newAdmin, "AdminPassword123!"); // !!! IMPORTANT: CHANGE THIS PASSWORD IN PRODUCTION !!!
                if (result.Succeeded)
                {
                    // Assign the "Admin" role to the new admin user
                    await _userManager.AddToRoleAsync(newAdmin, "Admin");
                    return true; // Admin account created
                }
                return false; // Failed to create admin user
            }
            return false; // Admin account already exists
        }

        /// <summary>
        /// Assigns a specified role to a user.
        /// </summary>
        /// <param name="user">The ApplicationUser to whom the role will be assigned.</param>
        /// <param name="roleName">The name of the role to assign (e.g., "Admin", "User").</param>
        /// <returns>True if the role was assigned successfully, false otherwise.</returns>
        public async Task<bool> AssignRoleToUserAsync(ApplicationUser user, string roleName)
        {
            // Ensure the role exists, create it if not
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            // Add the user to the specified role
            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        /// <summary>
        /// Retrieves a list of all application users with their permissions and roles.
        /// </summary>
        /// <returns>A list of UserDto objects containing user details, permissions, and roles.</returns>
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList(); // Get all ApplicationUsers
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Get roles for each user
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    CanViewDocuments = user.CanViewDocuments,
                    CanUploadDocuments = user.CanUploadDocuments,
                    Roles = roles.ToList() // Convert roles to a List<string>
                });
            }
            return userDtos;
        }


        /// <summary>
        /// Generates a JSON Web Token (JWT) for the given user, including their claims and roles.
        /// </summary>
        /// <param name="user">The ApplicationUser for whom to generate the token.</param>
        /// <returns>A JWT string.</returns>
        private string GenerateJwtToken(ApplicationUser user)
        {
            // Create a list of claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), // User's unique ID
                new Claim(ClaimTypes.Name, user.UserName),     // User's username
                new Claim("CanViewDocuments", user.CanViewDocuments.ToString()), // Custom permission claim
                new Claim("CanUploadDocuments", user.CanUploadDocuments.ToString()) // Custom permission claim
            };

            // Add user roles as claims
            var userRoles = _userManager.GetRolesAsync(user).Result; // Synchronously get roles for token generation
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Get JWT secret key from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            // Create signing credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // Set token expiration
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            // Write the token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
