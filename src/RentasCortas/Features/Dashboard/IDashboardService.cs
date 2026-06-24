namespace RentasCortas.Features.Dashboard;

public interface IDashboardService
{
    Task<DashboardResponseDTO> GetMetricsAsync(Guid ownerId, DateOnly? startDate, DateOnly? endDate);
}
