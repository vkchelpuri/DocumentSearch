using DocumentQnA.Api.Data;
using DocumentQnA.Api.Models; 
using DocumentQnA.Api.Services; 
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.IdentityModel.Tokens;
using System.Text; 
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; 
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>() 
.AddDefaultTokenProviders(); 

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Validate the server that created the token
        ValidateAudience = true, // Validate the recipient of the token is authorized to receive it
        ValidateLifetime = true, // Validate the token's expiration date
        ValidateIssuerSigningKey = true, // Validate the signing key
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Issuer from appsettings.json
        ValidAudience = builder.Configuration["Jwt:Audience"], // Audience from appsettings.json
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Key from appsettings.json
    };
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Policy for users who can view documents (based on custom claim)
    options.AddPolicy("CanViewDocumentsPolicy", policy =>
        policy.RequireClaim("CanViewDocuments", "True") // Requires the custom claim "CanViewDocuments" to be "True"
              .RequireAuthenticatedUser()); // Ensures the user is authenticated

    // Policy for users who can upload documents (based on custom claim)
    options.AddPolicy("CanUploadDocumentsPolicy", policy =>
        policy.RequireClaim("CanUploadDocuments", "True") // Requires the custom claim "CanUploadDocuments" to be "True"
              .RequireAuthenticatedUser()); // Ensures the user is authenticated

    // Policy for administrators (based on role)
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin") // Requires the user to be in the "Admin" role
              .RequireAuthenticatedUser()); // Ensures the user is authenticated
});


// Add controllers service
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DocumentQnA.Api", Version = "v1" });

    // Configure Swagger to use JWT Bearer authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // Lowercase "bearer" is important
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, new string[] { }}
    });

    // FIX: Resolve schemaId conflict for nested DTOs
    c.CustomSchemaIds(type =>
    {
        // If the type is a nested class, use its full name including the declaring type
        if (type.IsNested)
        {
            return $"{type.DeclaringType.Name}.{type.Name}";
        }
        // Otherwise, use the default name
        return type.Name;
    });
});

// Register custom services
builder.Services.AddScoped<ITextExtractor, TextExtractor>();
builder.Services.AddHttpClient<IGeminiServices, GeminiServices>(); // This is the fix for HttpClient
builder.Services.AddScoped<IAuthService, AuthService>(); // Register the AuthService

var app = builder.Build();

// Seed initial admin user if not exists
// This block ensures that an admin account is created when the application starts
// if one doesn't already exist.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var authService = services.GetRequiredService<IAuthService>();
        await authService.CreateAdminAccountIfNotExist();
        Console.WriteLine("Admin account seeding checked/completed.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the admin account.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
