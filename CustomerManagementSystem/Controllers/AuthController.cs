using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebApp.Entity.Dto;
using WebApp.Services.Repository;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IMemoryCache _memoryCache;

        public AuthController(IAuthRepository authRepository, IMemoryCache memoryCache)
        {
            _authRepository = authRepository;
            _memoryCache = memoryCache;

        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            var result = await _authRepository.SendOtpAsync(dto);
            if (result.Contains("success"))
                return Ok(new { message = result });
            return BadRequest(new { message = result });
        }

        [HttpPost("verify-register")]
        public async Task<IActionResult> VerifyAndRegister([FromBody] VerifyRegisterDto dto)
        {
            var result = await _authRepository.VerifyAndRegisterAsync(dto);
            if (result.Contains("successful"))
                return Ok(new { message = result });
            return BadRequest(new { message = result });
        }

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginDto dto)
        //{
        //    var response = await _authRepository.LoginAsync(dto);
        //    if (response == null)
        //        return Unauthorized(new { message = "Invalid credentials" });
        //    return Ok(response);
        //}
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _authRepository.LoginAsync(dto);
                if (response == null)
                    return Unauthorized(new { message = "Invalid credentials" });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message, StackTrace = ex.StackTrace });
            }
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Get the UserId from the JWT claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token or user not logged in" });

            // Remove the SID from memory cache
            _memoryCache.Remove($"sid_{userId}");

            //  remove other per-user caches
            _memoryCache.Remove($"menus_{userId}");

            return Ok(new { message = "Logged out successfully" });
        }

    }
}
