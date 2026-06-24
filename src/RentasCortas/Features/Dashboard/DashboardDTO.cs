namespace RentasCortas.Features.Dashboard;

public class DashboardFilterDTO
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class DashboardResponseDTO
{
    public decimal TotalRevenue { get; set; }
    public Dictionary<string, int> ReservationsByStatus { get; set; } = new();
    public List<OccupancyDTO> OccupancyByProperty { get; set; } = [];
    public TopPropertyDTO? TopProperty { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}

public class OccupancyDTO
{
    public Guid InmuebleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ReservedDays { get; set; }
    public int TotalDays { get; set; }
    public decimal OccupancyRate { get; set; }
}

public class TopPropertyDTO
{
    public Guid InmuebleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
}
