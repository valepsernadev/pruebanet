namespace RentasCortas.Features.Reportes;

public class ReporteFilterDTO
{
    public Guid? InmuebleId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class ReporteResultDTO
{
    public byte[] FileContent { get; set; } = [];
    public string FileName { get; set; } = string.Empty;
}
