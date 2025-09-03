namespace CivicFlow.Domain;

public enum Role { Citizen, Officer, Admin }
public enum TicketPriority { Low, Medium, High, Critical }
public enum TicketStatus { New, InProgress, Resolved, Closed }

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public Role Role { get; set; } = Role.Citizen;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = default!;

    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public DateTimeOffset? DueAt { get; set; }
    public string? Location { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

public class Attachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = default!;

    public string PathOrUrl { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public long Size { get; set; }
    public string ContentType { get; set; } = default!;
    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = default!;
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public Guid ActorId { get; set; }
    public User Actor { get; set; } = default!;

    public string Action { get; set; } = default!;
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class CivicEntry
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Resolved { get; set; } = false;
}