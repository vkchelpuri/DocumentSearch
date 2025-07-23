using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using System.Text; 
using DocumentQnA.Api.Models; 

namespace DocumentQnA.Api.Services
{

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager; 
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
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
        public async Task<(bool Success, string Token, string Message)> RegisterUserAsync(string username, string password, bool isAdmin = false)
        {
            var user = new ApplicationUser { UserName = username, Email = $"{username}@example.com" }; 
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                if (isAdmin)
                {
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }
                    await _userManager.AddToRoleAsync(user, "Admin");
                    user.CanViewDocuments = true;
                    user.CanUploadDocuments = true;
                    await _userManager.UpdateAsync(user); 
                }
                else
                {
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }
                    await _userManager.AddToRoleAsync(user, "User");
                }

                var token = GenerateJwtToken(user);
                return (true, token, "Registration successful.");
            }

            return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        /// <summary>
        /// Authenticates a user with the provided username and password.
        /// </summary>
        public async Task<(bool Success, string Token, string Message)> LoginAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return (false, null, "Invalid credentials.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);
                return (true, token, "Login successful.");
            }

            return (false, null, "Invalid credentials.");
        }

        /// <summary>
        /// Retrieves an ApplicationUser by their unique ID.
        /// </summary>
        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        /// <summary>
        /// Retrieves an ApplicationUser by their username.
        /// </summary>
        public async Task<ApplicationUser> GetUserByUsernameAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        /// <summary>
        /// Updates the document viewing and uploading permissions for a specific user.
        /// This operation is typically restricted to administrators.
        /// </summary>
        public async Task<bool> UpdateUserPermissionsAsync(string userId, bool canViewDocuments, bool canUploadDocuments)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false; 
            }

            user.CanViewDocuments = canViewDocuments;
            user.CanUploadDocuments = canUploadDocuments;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        /// <summary>
        /// Creates an initial admin account if one does not already exist.
        /// This is useful for bootstrapping the application with a default administrator.
        /// </summary>
        public async Task<bool> CreateAdminAccountIfNotExist()
        {
            var adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
            if (!adminRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var adminUser = await _userManager.FindByNameAsync("admin"); 
            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@example.com", 
                    CanViewDocuments = true,
                    CanUploadDocuments = true
                };
                var result = await _userManager.CreateAsync(newAdmin, "AdminPassword123!"); 
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newAdmin, "Admin");
                    return true; 
                }
                return false; 
            }
            return false;
        }

        /// <summary>
        /// Assigns a specified role to a user.
        /// </summary>
        public async Task<bool> AssignRoleToUserAsync(ApplicationUser user, string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        /// <summary>
        /// Retrieves a list of all application users with their permissions and roles.
        /// </summary>
        /// <returns>A list of UserDto objects containing user details, permissions, and roles.</returns>
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList(); 
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); 
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    CanViewDocuments = user.CanViewDocuments,
                    CanUploadDocuments = user.CanUploadDocuments,
                    Roles = roles.ToList() 
                });
            }
            return userDtos;
        }


        /// <summary>
        /// Generates a JSON Web Token (JWT) for the given user, including their claims and roles.
        /// </summary>
        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                new Claim(ClaimTypes.Name, user.UserName),     
                new Claim("CanViewDocuments", user.CanViewDocuments.ToString()), 
                new Claim("CanUploadDocuments", user.CanUploadDocuments.ToString()) 
            };

            var userRoles = _userManager.GetRolesAsync(user).Result; 
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
