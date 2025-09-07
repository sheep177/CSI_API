using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CivicFlow.Domain;
using CivicFlow.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace CivicFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(AppDbContext db, IConfiguration config, EmailService email) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            token,
            user = new { user.Id, user.Email, role = user.Role.ToString() }
        });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (!IsValidEmail(req.Email))
            return BadRequest(new { message = "Invalid email format" });

        if (req.Password.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters" });

        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "Email already exists" });

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var user = new User
        {
            Email = req.Email,
            PasswordHash = hashedPassword,
            Role = Role.Citizen
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            message = "Registration successful",
            token,
            user = new { user.Id, user.Email, role = user.Role.ToString() }
        });
    }

    [HttpPost("send-verification")]
    public async Task<IActionResult> SendVerificationCode([FromBody] EmailOnlyRequest req)
    {
        var old = await db.EmailVerifications
            .Where(e => e.Email == req.Email && !e.IsUsed)
            .ToListAsync();
        db.EmailVerifications.RemoveRange(old);

        var code = GenerateRandomCode(6);

        var entry = new EmailVerification
        {
            Email = req.Email,
            Code = code
        };

        db.EmailVerifications.Add(entry);
        await db.SaveChangesAsync();

        await email.SendEmailAsync(
            to: req.Email,
            subject: "Your CivicFlow verification code",
            body: $"<p>Your verification code is <strong>{code}</strong>. It will expire in 10 minutes.</p>"
        );

        return Ok(new { message = "Verification code sent" });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest req)
    {
        var entry = await db.EmailVerifications
            .FirstOrDefaultAsync(e => e.Email == req.Email && e.Code == req.Code && !e.IsUsed);

        if (entry == null || entry.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return BadRequest(new { message = "Invalid or expired code" });
        }

        entry.IsUsed = true;
        await db.SaveChangesAsync();

        return Ok(new { message = "Email verified successfully" });
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var code = new StringBuilder(length);
        foreach (var b in bytes)
            code.Append(chars[b % chars.Length]);
        return code.ToString();
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "YourFallbackSecretKey123!");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
    

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password);
    public record EmailOnlyRequest(string Email);
    public record VerifyEmailRequest(string Email, string Code);
    
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null)
        {
            return NotFound(new { message = "Email not found" });
        }

        var token = Guid.NewGuid().ToString();
        var reset = new PasswordResetToken
        {
            Email = req.Email,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        db.PasswordResetTokens.Add(reset);
        await db.SaveChangesAsync();

        var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={token}";

        await email.SendEmailAsync(
            to: req.Email,
            subject: "Reset your password",
            body: $"Click the link to reset your password: <a href=\"{resetLink}\">{resetLink}</a>"
        );

        return Ok(new { message = "Reset link sent to your email." });
    }
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var entry = await db.PasswordResetTokens.FirstOrDefaultAsync(p =>
            p.Token == req.Token && !p.Used && p.ExpiresAt > DateTime.UtcNow);

        if (entry == null)
        {
            return BadRequest(new { message = "Invalid or expired token" });
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == entry.Email);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        entry.Used = true;

        await db.SaveChangesAsync();
        return Ok(new { message = "Password has been reset successfully." });
    }
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword);
}