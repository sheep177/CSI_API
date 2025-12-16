
using CivicFlow.Domain;                        // 引入定义实体类（User, Ticket 等）的命名空间
using Microsoft.EntityFrameworkCore;           // 引入 Entity Framework Core 的核心功能（DbContext 等）


namespace CivicFlow.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  

    public DbSet<User> Users => Set<User>(); 
    public DbSet<Ticket> Tickets => Set<Ticket>(); 
    public DbSet<Attachment> Attachments => Set<Attachment>(); 
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique(); 
            e.Property(x => x.Role) 
                .HasConversion<string>()
                .HasMaxLength(20); 
        });

        // 配置 Ticket 表
        modelBuilder.Entity<Ticket>(e =>
        {
            e.Property(t => t.RowVersion)
                .IsRowVersion(); 

            e.Property(t => t.Priority)
                .HasConversion<string>() 
                .HasMaxLength(20); 

            e.Property(t => t.Status)
                .HasConversion<string>() 
                .HasMaxLength(20);

            e.HasIndex(t => t.Status); 
            e.HasIndex(t => t.CreatedAt); 
           
            e.HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Attachment>(e =>
        {
            e.HasIndex(a => a.TicketId); 
        });

        
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.TicketId); 
        });
    }
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
    
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
}
