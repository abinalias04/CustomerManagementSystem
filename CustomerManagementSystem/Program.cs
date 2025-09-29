using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WebApp.Api.Repositories;
using WebApp.Entity.Data;
using WebApp.Entity.Models;
using WebApp.Entity.Validators;
using WebApp.Services.Repository;

var builder = WebApplication.CreateBuilder(args);

// ------------------ Configure Services ------------------

// 1. Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity (User + Role Management)
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 3. Controllers + JSON + FluentValidation
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep PascalCase
        options.JsonSerializerOptions.WriteIndented = true;        // Pretty JSON
    })
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<GenerateOtpDtoValidator>());


// 4. Enable CORS (for Angular frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// 5. JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = ClaimTypes.Role
    };
});

// 6. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// 7. Dependency Injection for Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IReturnRepository, ReturnRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();



// 8. EmailService + MemoryCache
builder.Services.AddScoped<EmailService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

// ------------------ Middleware ------------------
//if (app.Environment.IsDevelopment())
//{

//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<JwtClaimsMiddleware>();

// Enable wwwroot for serving images
app.UseStaticFiles();

app.MapControllers();

//// ------------------ Seed Roles & Admin ------------------
//await SeedRolesAndAdminAsync(app);

app.Run();

//// ------------------ Seed Method ------------------
//static async Task SeedRolesAndAdminAsync(WebApplication app)
//{
//    using var scope = app.Services.CreateScope();
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
//    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

//    // ----- Seed Default Admin -----
//    var adminEmail = "admin@gmail.com";
//    var adminPassword = "Admin@123"; // ⚠️ In production, store in appsettings or user secrets

//    var adminUser = await userManager.FindByEmailAsync(adminEmail);
//    if (adminUser == null)
//    {
//        var newAdmin = new ApplicationUser
//        {
//            UserName = adminEmail,
//            Email = adminEmail,
//            FullName = "System Admin",
//            Role = UserRole.Admin,
//            Status = UserStatus.Active,
//            Badge = UserBadge.Gold,
//            DateJoined = DateTime.UtcNow,
//            EmailConfirmed = true,
//            CreatedAt = DateTime.UtcNow,
//            Address = "Head Office",
//            PhoneNumber = "0000000000"
//        };

//        var result = await userManager.CreateAsync(newAdmin, adminPassword);
//        if (result.Succeeded)
//        {
//            await userManager.AddToRoleAsync(newAdmin, UserRole.Admin.ToString());
//            Console.WriteLine($"Seeded Default Admin: {adminEmail}");
//        }
//        else
//        {
//            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
//            throw new Exception($"Failed to create default admin: {errors}");
//        }
//    }
//}
