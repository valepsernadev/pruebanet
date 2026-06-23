namespace RentasCortas.Features.Inmuebles;

public class CreateInmuebleDTO
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
}

public class UpdateInmuebleDTO
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class InmuebleResponseDTO
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<ImageResponseDTO> Images { get; set; } = [];
}

public class InmuebleListResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MainImage { get; set; }
}

public class ImageResponseDTO
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class InmuebleFilterDTO
{
    public string? Location { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public DateOnly? AvailableTo { get; set; }
}
