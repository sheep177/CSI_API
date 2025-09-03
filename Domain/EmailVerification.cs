// File: Domain/EmailVerification.cs

using System;

namespace CivicFlow.Domain;

public class EmailVerification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(10);

    public bool IsUsed { get; set; } = false;
}