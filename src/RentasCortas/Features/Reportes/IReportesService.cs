namespace RentasCortas.Features.Reportes;

public interface IReportesService
{
    Task<ReporteResultDTO> GenerateReportAsync(Guid ownerId, Guid? inmuebleId, DateOnly? startDate, DateOnly? endDate);
}
