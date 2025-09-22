using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Entity.Dto;
using WebApp.Services.Repository;

namespace WebApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReturnController : ControllerBase
    {
        private readonly IReturnRepository _returnRepository;

        public ReturnController(IReturnRepository returnRepository)
        {
            _returnRepository = returnRepository;
        }

        // 1) Create return request
        [HttpPost("request")]
        [Authorize]
        public async Task<IActionResult> CreateReturnRequest([FromBody] CreateReturnRequestDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user.");

            var returnId = await _returnRepository.CreateReturnRequestAsync(dto, userId);
            return Ok(new { ReturnRequestId = returnId });
        }

        // 2) Approve return
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(adminIdClaim, out var adminId))
                return Unauthorized("Invalid admin.");

            var ok = await _returnRepository.ApproveReturnRequestAsync(id, adminId);
            return ok ? Ok(new { success = true }) : NotFound("Return request not found.");
        }

        // 3) Complete return
        [HttpPost("complete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Complete([FromBody] CompleteReturnDto dto)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(adminIdClaim, out var adminId))
                return Unauthorized("Invalid admin.");

            var summary = await _returnRepository.CompleteReturnAsync(dto, adminId);
            return summary != null ? Ok(summary) : BadRequest("Return could not be completed.");
        }
        [HttpGet("all")]
        [Authorize(Roles = "Admin,SuperAdmin")] // usually only admin-level roles can see all returns
        public async Task<IActionResult> GetAll([FromQuery] ReturnQueryParameters parameters)
        {
            var result = await _returnRepository.GetAllReturnsAsync(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _returnRepository.GetReturnByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("my")]
        [Authorize] // any logged-in user
        public async Task<IActionResult> GetMyReturns([FromQuery] ReturnQueryParameters parameters)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user.");

            var result = await _returnRepository.GetMyReturnsAsync(userId, parameters);
            return Ok(result);
        }

    }
}
