namespace RentasCortas.Features.Favoritos;

public class FavoritoResponseDTO
{
    public Guid Id { get; set; }
    public Guid InmuebleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public List<string> Images { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}