using CivicFlow.Domain;
using CivicFlow.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CivicFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // 所有接口都需要通过 JWT 授权访问
public class TicketsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    //这个方法就是asp.net controller的方法来响应前段发来的http get请求
    // await：等待数据库查询结果
// db.Tickets：访问数据库中的 Tickets 表
// AsNoTracking()：不启用 Entity Framework 的更改追踪（只读操作，提升性能）
// Select(...)：只取我们需要的字段（Id, Title, Status 等），而不是整个 Ticket 对象
// ToListAsync()：把结果转成一个 List，然后异步返回
    public async Task<IActionResult> Get() =>//定义异步方法返回json响应，自动绑定ticket实体 domain ticket
        Ok(await db.Tickets.AsNoTracking()//ok代表返回200OK状态+json内容，asnotracking是状态追踪，只读不编辑提升性能
            .Select(t => new { t.Id, t.Title, t.Status, t.Priority, t.CreatedAt })
            //。select就是选择器，只要取我想要的字段，然后会自动编程json返回
            .ToListAsync());

    // 这个方法是 ASP.NET Core 控制器的方法，用来响应前端发来的 HTTP POST 请求（例如：/api/tickets）
// 用来“新增”一个 ticket 工单
    [HttpPost]
    // 定义一个异步方法，返回类型是 IActionResult（表示 HTTP 响应）
// async 表示方法内部会用 await 做异步操作，不会卡住服务器线程
    public async Task<IActionResult> Create([FromBody] Ticket ticket)
    {
        // [FromBody] 表示这个参数 ticket 会从前端发来的请求 body 中读取 JSON 自动绑定（通过模型绑定）
        // 把接收到的 ticket 实例加入到数据库的 Tickets 表中
        db.Tickets.Add(ticket);
        // 保存更改到数据库，异步执行（避免阻塞线程）
        await db.SaveChangesAsync();
        // 返回 HTTP 201 Created 响应，表示“创建成功”
        // CreatedAtAction 的意思是：告诉客户端“你创建的资源在 Get() 这个 API 里可以查到”
        // 它还会自动在响应头里加上 Location: /api/tickets/{id}
        // nameof(Get)：引用上面的 Get 方法名（作为 location 指向）
        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ticket);
    } //newid是// 路由参数（匿名对象），告诉客户端这个 ticket 的 ID 是多少
    // 响应 body 中也带回刚创建的 ticket 实例
    
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Ticket updatedTicket)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound();

        // 更新字段
        ticket.Title = updatedTicket.Title;
        ticket.Description = updatedTicket.Description;
        ticket.Status = updatedTicket.Status;
        ticket.Priority = updatedTicket.Priority;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();
        return NoContent(); // 204，无内容返回，但表示成功
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
}