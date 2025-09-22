//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Caching.Memory;
//using WebApp.Entity.Dto;
//using WebApp.Entity.Models;
//using Microsoft.AspNetCore.Identity;

//[ApiController]
//[Route("api/[controller]")]
//public class OtpController : ControllerBase
//{
//    private readonly EmailService _emailService;
//    private readonly IMemoryCache _cache;
//    private readonly UserManager<ApplicationUser> _userManager;

//    public OtpController(EmailService emailService, IMemoryCache cache, UserManager<ApplicationUser> userManager)
//    {
//        _emailService = emailService;
//        _cache = cache;
//        _userManager = userManager;
//    }

//    [HttpPost("send")]
//    public IActionResult SendOtp([FromBody] GenerateOtpDto dto)
//    {
//        var attemptKey = $"{dto.Email}_Attempts";
//        if (_cache.TryGetValue(attemptKey, out int attempts) && attempts >= 3)
//        {
//            return BadRequest(new { success = false, message = "Too many OTP requests. Try again later." });
//        }

//        _cache.Set(attemptKey, (attempts + 1), TimeSpan.FromMinutes(15));

//        var otp = new Random().Next(100000, 999999).ToString();
//        _cache.Set(dto.Email, otp, TimeSpan.FromMinutes(5));

//        string subject = "CMS - Email Verification OTP";
//        string body = $@"
//            <p>Thank you for registering with <b>CMS</b>.</p>
//            <p>Your One-Time Password (OTP) is:</p>
//            <h2 style='color:blue;'>{otp}</h2>
//            <p>⚠️ This code is valid for <b>5 minutes</b>.</p>
//        ";

//        Task.Run(() => _emailService.SendEmailAsync(dto.Email, subject, body));

//        return Ok(new { success = true, message = "OTP request accepted! Please check your email." });
//    }

//    [HttpPost("verify")]
//    public IActionResult VerifyOtp([FromBody] VerifyOtpDto dto)
//    {
//        if (!_cache.TryGetValue(dto.Email, out string cachedOtp))
//        {
//            return BadRequest(new { success = false, message = "OTP expired or not found!" });
//        }

//        if (cachedOtp == dto.Otp)
//        {
//            _cache.Remove(dto.Email);

//            //  Mark this email as verified in cache for short period
//            _cache.Set($"{dto.Email}_Verified", true, TimeSpan.FromMinutes(10));

//            return Ok(new { success = true, status = "Verified", message = "OTP verified successfully!" });
//        }

//        return BadRequest(new { success = false, message = "Invalid OTP!" });
//    }

//}
