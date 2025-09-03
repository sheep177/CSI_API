// 引入使用到的命名空间
using CivicFlow.Domain;                        // 引入定义实体类（User, Ticket 等）的命名空间
using Microsoft.EntityFrameworkCore;           // 引入 Entity Framework Core 的核心功能（DbContext 等）

// 定义命名空间，通常与项目结构对应
namespace CivicFlow.Infrastructure;

/// <summary>
/// AppDbContext 是你数据库的“上下文”，也就是数据库的主接口，
/// 用来读写数据表（User、Ticket、Attachment、AuditLog）
/// 它继承自 EF Core 提供的 DbContext 基类
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // 定义数据库中的每一张表（DbSet 对象）
    // 每个 DbSet<T> 代表数据库中一张表，T 是对应的实体类

    public DbSet<User> Users => Set<User>(); // 用户表
    public DbSet<Ticket> Tickets => Set<Ticket>(); // 工单表
    public DbSet<Attachment> Attachments => Set<Attachment>(); // 附件表
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>(); // 审计日志表

    /// <summary>
    /// 当 EF Core 在首次创建数据库表结构时，会调用这个方法来进一步配置字段、索引等内容
    /// 相当于 Code First 模式下的“自定义建表规则”
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 配置 User 表
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique(); // Email 字段创建唯一索引，防止重复
            e.Property(x => x.Role) // Role 是枚举，转成字符串存储
                .HasConversion<string>() // 将 enum 类型存成 string（如 "Admin"）
                .HasMaxLength(20); // 限制字段最大长度为 20 字符
        });

        // 配置 Ticket 表
        modelBuilder.Entity<Ticket>(e =>
        {
            e.Property(t => t.RowVersion)
                .IsRowVersion(); // 开启乐观并发控制（用于更新时冲突检测）

            e.Property(t => t.Priority)
                .HasConversion<string>() // 把枚举 Priority 存为 string（如 "High"）
                .HasMaxLength(20); // 限定字符串长度

            e.Property(t => t.Status)
                .HasConversion<string>() // 同样把枚举 Status 存为 string
                .HasMaxLength(20);

            e.HasIndex(t => t.Status); // 为 Status 添加索引，加快过滤速度
            e.HasIndex(t => t.CreatedAt); // 为 CreatedAt 添加索引，加快时间段筛选
            // ✅ 指定双重用户关系
            e.HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 配置 Attachment 表（为 TicketId 添加索引）
        modelBuilder.Entity<Attachment>(e =>
        {
            e.HasIndex(a => a.TicketId); // 快速按 TicketId 查附件
        });

        // 配置 AuditLog 表（为 TicketId 添加索引）
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.TicketId); // 快速按 TicketId 查日志
        });
    }
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
}
