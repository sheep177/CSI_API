using CivicFlow.Domain;
using CivicFlow.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CivicFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TicketsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        IQueryable<Ticket> query = db.Tickets.AsNoTracking();

        if (user.Role == Role.Citizen)
        {
            query = query.Where(t => t.CreatedById == userId);
        }
        else if (user.Role == Role.Officer)
        {
            query = query.Where(t => t.AssignedToId == userId || t.CreatedById == userId);
        }

        var result = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Status,
                t.Priority,
                t.CreatedAt,
                CreatedBy = t.CreatedBy.FullName,
                AssignedTo = t.AssignedTo != null ? t.AssignedTo.FullName : null
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Ticket ticket)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        ticket.Id = Guid.NewGuid();
        ticket.CreatedById = userId;
        ticket.CreatedAt = DateTimeOffset.UtcNow;
        ticket.Status = TicketStatus.New;

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Ticket updatedTicket)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound();

        ticket.Title = updatedTicket.Title;
        ticket.Description = updatedTicket.Description;
        ticket.Status = updatedTicket.Status;
        ticket.Priority = updatedTicket.Priority;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound();

        db.Tickets.Remove(ticket);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignRequest req)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound();

        var officer = await db.Users.FirstOrDefaultAsync(u => u.Id == req.OfficerId && u.Role == Role.Officer);
        if (officer == null)
            return BadRequest(new { message = "Invalid officer ID" });

        ticket.AssignedToId = officer.Id;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();
        return Ok(new { message = "Ticket assigned successfully." });
    }

    public record AssignRequest(Guid OfficerId);
}