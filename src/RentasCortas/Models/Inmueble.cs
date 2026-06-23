namespace RentasCortas.Models;

public class Inmueble
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Usuario Owner { get; set; } = null!;
    public List<PropertyImage> Images { get; set; } = [];
}