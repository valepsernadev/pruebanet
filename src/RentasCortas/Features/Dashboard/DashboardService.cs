using Microsoft.EntityFrameworkCore;
using RentasCortas.Data;

namespace RentasCortas.Features.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponseDTO> GetMetricsAsync(Guid ownerId, DateOnly? startDate, DateOnly? endDate)
    {
        try
        {
            var periodEnd = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var periodStart = startDate ?? periodEnd.AddDays(-30);
            var totalDays = periodEnd.DayNumber - periodStart.DayNumber;
            if (totalDays <= 0)
                throw new ArgumentException("La fecha de inicio debe ser anterior a la fecha de fin");

            var ownerInmuebles = await _context.Inmuebles
                .Where(i => i.OwnerId == ownerId)
                .ToListAsync();

            var inmuebleIds = ownerInmuebles.Select(i => i.Id).ToList();

            var reservas = await _context.Reservas
                .Where(r => inmuebleIds.Contains(r.InmuebleId)
                    && r.CheckIn < periodEnd
                    && r.CheckOut > periodStart)
                .ToListAsync();

            var totalRevenue = reservas
                .Where(r => r.Status == "confirmed" || r.Status == "completed")
                .Sum(r => r.TotalPrice);

            var reservationsByStatus = reservas
                .GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var status in new[] { "pending", "confirmed", "cancelled", "completed" })
            {
                reservationsByStatus.TryAdd(status, 0);
            }

            var occupancyByProperty = ownerInmuebles.Select(inmueble =>
            {
                var propertyReservas = reservas
                    .Where(r => r.InmuebleId == inmueble.Id
                        && (r.Status == "confirmed" || r.Status == "completed"))
                    .ToList();

                var reservedDays = 0;
                foreach (var r in propertyReservas)
                {
                    var effectiveStart = r.CheckIn < periodStart ? periodStart : r.CheckIn;
                    var effectiveEnd = r.CheckOut > periodEnd ? periodEnd : r.CheckOut;
                    var days = effectiveEnd.DayNumber - effectiveStart.DayNumber;
                    if (days > 0) reservedDays += days;
                }

                var rate = totalDays > 0 ? Math.Round((decimal)reservedDays / totalDays * 100, 2) : 0;

                return new OccupancyDTO
                {
                    InmuebleId = inmueble.Id,
                    Title = inmueble.Title,
                    ReservedDays = reservedDays,
                    TotalDays = totalDays,
                    OccupancyRate = rate
                };
            }).ToList();

            var topProperty = reservas
                .Where(r => r.Status == "confirmed" || r.Status == "completed")
                .GroupBy(r => r.InmuebleId)
                .Select(g => new
                {
                    InmuebleId = g.Key,
                    Revenue = g.Sum(r => r.TotalPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .FirstOrDefault();

            TopPropertyDTO? topPropertyDto = null;
            if (topProperty != null)
            {
                var inmueble = ownerInmuebles.First(i => i.Id == topProperty.InmuebleId);
                topPropertyDto = new TopPropertyDTO
                {
                    InmuebleId = inmueble.Id,
                    Title = inmueble.Title,
                    TotalRevenue = topProperty.Revenue
                };
            }

            return new DashboardResponseDTO
            {
                TotalRevenue = totalRevenue,
                ReservationsByStatus = reservationsByStatus,
                OccupancyByProperty = occupancyByProperty,
                TopProperty = topPropertyDto,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            throw new InvalidOperationException("Error al obtener las métricas del dashboard", ex);
        }
    }
}
