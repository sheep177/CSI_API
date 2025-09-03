using CivicFlow.Domain;
using CivicFlow.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(AppDbContext db) : ControllerBase
{
    // 获取所有用户（注意不返回密码！）
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await db.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.Email, u.Role })
            .ToListAsync());

    // 创建用户（注册），假设密码已加密（这里只是示范）
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        // 检查邮箱是否重复
        if (await db.Users.AnyAsync(u => u.Email == user.Email))
            return BadRequest("Email already exists.");

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = user.Id }, new { user.Id, user.Email, user.Role });
    }

    // 修改用户角色（假设我们只允许改角色）
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Role role)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.Role = role;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // 删除用户
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }
}