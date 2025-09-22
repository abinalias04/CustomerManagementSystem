using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApp.Entity.Data;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;

namespace WebApp.Services.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IMemoryCache _memoryCache;

        public AuthRepository(UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole<int>> roleManager,
                              IConfiguration config,
                              AppDbContext context,
                               EmailService emailService,
                               IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _context = context;
            _emailService = emailService;
            _memoryCache = memoryCache;
        }

        public async Task<string> SendOtpAsync(SendOtpDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return " User already exists!";

            var recentAttempts = await _context.UserOtps
                .Where(o => o.Email == dto.Email && o.CreatedAt > DateTime.UtcNow.AddMinutes(-15))
                .CountAsync();

            if (recentAttempts >= 3)
                return " Too many OTP requests. Please try again after 15 minutes.";

            var otp = new Random().Next(100000, 999999).ToString();

            var userOtp = new UserOtp
            {
                Email = dto.Email,
                Otp = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Status = OtpStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserOtps.Add(userOtp);
            await _context.SaveChangesAsync();

            string subject = "CMS - Email Verification OTP";
            string body = $@"
        <p>Thank you for registering with <b>CMS</b>.</p>
        <p>Your One-Time Password (OTP) is:</p>
        <h2 style='color:blue;'>{otp}</h2>
        <p>⚠️ This code is valid for <b>5 minutes</b>.</p>
    ";

            Task.Run(() => _emailService.SendEmailAsync(dto.Email, subject, body));

            return " OTP sent successfully!";
        }


        public async Task<string> VerifyAndRegisterAsync(VerifyRegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return "Password and ConfirmPassword do not match";

            // Fetch latest OTP for email
            var userOtp = await _context.UserOtps
                .Where(o => o.Email == dto.Email && o.Status == OtpStatus.Pending)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (userOtp == null || userOtp.Otp != dto.Otp || userOtp.ExpiresAt < DateTime.UtcNow)
                return "Invalid or expired OTP";

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return "User already exists!";

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true,
                Status = UserStatus.Active,
                Role = UserRole.Customer
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return string.Join(", ", result.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(user, "Customer");

            // Mark OTP as verified
            userOtp.Status = OtpStatus.Verified;
            await _context.SaveChangesAsync();

            return "Registration successful!";
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.Status != UserStatus.Active)
                return null;

            var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!validPassword)
                return null;

            var token = await GenerateJwtToken(user);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            var roleEntity = await _roleManager.FindByNameAsync(user.Role.ToString());
            var menus = await _context.RoleMenus
                .Where(rm => rm.RoleId == roleEntity.Id)
                .Select(rm => new MenuDto
                {
                    Id = rm.Menu.MenuId,
                    Name = rm.Menu.Name,
                    Path = rm.Menu.Path
                })
                .ToListAsync();

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                Role = role,
                Menus = menus
            };
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var sid = Guid.NewGuid().ToString();
            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),   // Standard JWT(Angular)
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),     // ASP.NET Core compatible(Internal)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("sid", sid),
            };

            _memoryCache.Set($"sid_{user.Id.ToString()}", sid);

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
