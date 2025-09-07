using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CivicFlow.Domain;

public class PasswordResetToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Email { get; set; }

    [Required]
    public string Token { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public bool Used { get; set; } = false;
}