using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebApp.Api.Repositories;


namespace WebApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _repo;

        public DashboardController(IDashboardRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            // middleware places userId and role into HttpContext.Items
            var userIdStr = HttpContext.Items["UserId"]?.ToString();
            var role = HttpContext.Items["UserRole"]?.ToString();

            if (string.IsNullOrEmpty(role))
                return Unauthorized("No role found in context");

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminDto = await _repo.GetAdminDashboardAsync();
                return Ok(adminDto);
            }
            if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                    return Unauthorized("No user id in context");

                var custDto = await _repo.GetCustomerDashboardAsync(userId);
                return Ok(custDto);
            }

            return Forbid("Unknown role");
        }
    }
}
