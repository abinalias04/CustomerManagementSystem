using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Services.Repository;
using WebApp.Entity.Dto;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // all endpoints require login
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseRepository _purchaseRepo;

        public PurchaseController(IPurchaseRepository purchaseRepo)
        {
            _purchaseRepo = purchaseRepo;
        }

        //  USER checkout
        [HttpPost("complete")]
        public async Task<ActionResult<PurchaseResultDto>> CompletePurchase()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User not logged in");

            int userId = int.Parse(userIdClaim);

            try
            {
                var result = await _purchaseRepo.CompletePurchaseAsync(userId);
                if (result == null)
                    return BadRequest("Unable to complete purchase. Cart may be empty or profile incomplete.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        //  ADMIN only - view all purchases
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPurchases([FromQuery] PurchaseQueryParameters parameters)
        {
            var result = await _purchaseRepo.GetAllPurchasesAsync(parameters);
            return Ok(result);
        }

        //  ADMIN only - view specific purchase
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPurchaseById(int id)
        {
            var result = await _purchaseRepo.GetPurchaseByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        //  USER only - my purchases
        [HttpGet("my")]
        public async Task<IActionResult> GetMyPurchases([FromQuery] PurchaseQueryParameters parameters)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User not logged in");

            int userId = int.Parse(userIdClaim);

            var result = await _purchaseRepo.GetMyPurchasesAsync(userId, parameters);
            return Ok(result);
        }
    }
}
