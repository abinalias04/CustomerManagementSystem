using Microsoft.AspNetCore.Mvc;
using WebApp.Entity.Dto;
using WebApp.Services.Repository;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileRepository _profileRepo;

    public ProfileController(IProfileRepository profileRepo)
    {
        _profileRepo = profileRepo;
    }

    private int GetUserIdFromContext()
        => int.Parse(HttpContext.Items["UserId"]!.ToString()!);

    //private string? GetUserRoleFromContext()
    //    => HttpContext.Items["UserRole"]?.ToString();

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserIdFromContext();
        var profile = await _profileRepo.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserIdFromContext();
        var success = await _profileRepo.CompleteProfileAsync(userId, dto);

        if (!success) return NotFound("User not found");
        return Ok(new { message = "Profile completed successfully" });
    }

    [HttpGet("pincode/{pincodeValue}")]
    public async Task<IActionResult> GetPincodeDetails(string pincodeValue)
    {
        var details = await _profileRepo.GetPincodeDetailsAsync(pincodeValue);
        if (details == null) return NotFound("Invalid Pincode");

        return Ok(details);
    }
}
