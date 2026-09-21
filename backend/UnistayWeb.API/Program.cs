using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.User;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Unistay Web API", Version = "v1" });
    // Fix: map IFormFile so Swagger can generate schema for file upload endpoints
    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<UserProfile, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<UnistayWeb.API.Identity.VietnameseIdentityErrorDescriber>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSecretKeyHere_ChangeInProduction_MinimumLength32Characters";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextjs", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Next.js default port
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); // needed if you use cookies/signalR
    });
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add SignalR for real-time messaging
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB for file uploads
}).AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null; // Keep PascalCase
});

// Register Custom Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IAccountService, UnistayWeb.BLL.Services.AccountService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IOnboardingService, UnistayWeb.BLL.Services.OnboardingService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IRoomService, UnistayWeb.BLL.Services.RoomService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IMarketplaceService, UnistayWeb.BLL.Services.MarketplaceService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IRoommateService, UnistayWeb.BLL.Services.RoommateService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IConnectionService, UnistayWeb.BLL.Services.ConnectionService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IProfileService, UnistayWeb.BLL.Services.ProfileService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IMessageService, UnistayWeb.BLL.Services.MessageService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.RentalAdvice.IRentalAdviceService, UnistayWeb.BLL.Services.RentalAdvice.RentalAdviceService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IFileUploadService, UnistayWeb.BLL.Services.LocalFileUploadService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IAiMatchingService, UnistayWeb.BLL.Services.AiMatchingService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IEmailService, UnistayWeb.BLL.Services.EmailService>();
builder.Services.AddScoped<UnistayWeb.BLL.Services.IZodiacService, UnistayWeb.BLL.Services.ZodiacService>();

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DatabaseSeeder.EnsureTablesExist(context);

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRolesAsync(roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Redirect root / to /swagger in development
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.UseExceptionHandler("/api/Home");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowNextjs");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR Hub
app.MapHub<UnistayWeb.API.Hubs.ChatHub>("/chatHub");

app.MapControllers();

app.Run();

static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Student", "Landlord", "Admin" };
    
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
