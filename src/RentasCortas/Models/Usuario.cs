namespace RentasCortas.Models;

public class Usuario
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public string KycStatus { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}