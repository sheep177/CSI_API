using CivicFlow.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CivicFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class MeController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Role,
            user.FullName,
            user.Phone,
            user.Address,
            user.DateOfBirth
        });
    }
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest req)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.FullName = req.FullName ?? user.FullName;
        user.Phone = req.Phone ?? user.Phone;
        user.Address = req.Address ?? user.Address;
        user.DateOfBirth = req.DateOfBirth ?? user.DateOfBirth;

        await db.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully" });
    }

    public record UpdateProfileRequest(
        string? FullName,
        string? Phone,
        string? Address,
        DateTime? DateOfBirth
    );
}