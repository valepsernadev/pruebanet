using Microsoft.EntityFrameworkCore;
using RentasCortas.Data;
using RentasCortas.Models;

namespace RentasCortas.Features.Reservas;

public class ReservasService : IReservasService
{
    private readonly AppDbContext _context;

    public ReservasService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReservaResponseDTO> CreateAsync(Guid guestId, CreateReservaDTO dto)
    {
        try
        {
            if (dto.CheckIn >= dto.CheckOut)
                throw new ArgumentException("La fecha de check-in debe ser anterior a la de check-out");

            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == dto.InmuebleId && i.Status == "active")
                ?? throw new KeyNotFoundException("Inmueble no encontrado o no disponible");

            if (inmueble.OwnerId == guestId)
                throw new ArgumentException("No puedes reservar tu propio inmueble");

            var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;

            var reserva = new Reserva
            {
                InmuebleId = dto.InmuebleId,
                GuestId = guestId,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                CheckInTime = new TimeOnly(14, 0),
                CheckOutTime = new TimeOnly(12, 0),
                TotalPrice = nights * inmueble.PricePerNight
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return await GetByIdInternalAsync(reserva.Id);
        }
        catch (Exception ex) when (ex is not ArgumentException and not KeyNotFoundException)
        {
            throw new InvalidOperationException("Error al crear la reserva", ex);
        }
    }

    public async Task<ReservaResponseDTO> ConfirmAsync(Guid ownerId, Guid reservaId)
    {
        try
        {
            var reserva = await _context.Reservas
                .Include(r => r.Inmueble)
                .Include(r => r.Guest)
                .FirstOrDefaultAsync(r => r.Id == reservaId)
                ?? throw new KeyNotFoundException("Reserva no encontrada");

            if (reserva.Inmueble.OwnerId != ownerId)
                throw new UnauthorizedAccessException("No tienes permiso para confirmar esta reserva");

            if (reserva.Status != "pending")
                throw new ArgumentException("Solo se pueden confirmar reservas en estado pending");

            // KYC obligatorio solo para la primera reserva del guest
            var hasCompletedReservations = await _context.Reservas
                .AnyAsync(r => r.GuestId == reserva.GuestId
                    && r.Id != reservaId
                    && (r.Status == "confirmed" || r.Status == "completed"));

            if (!hasCompletedReservations && reserva.Guest.KycStatus != "approved")
                throw new ArgumentException("El guest debe completar la verificación KYC antes de su primera reserva");

            // Anti double-booking
            var hasConflict = await _context.Reservas
                .AnyAsync(r => r.InmuebleId == reserva.InmuebleId
                    && r.Id != reservaId
                    && r.Status == "confirmed"
                    && r.CheckIn < reserva.CheckOut
                    && r.CheckOut > reserva.CheckIn);

            if (hasConflict)
                throw new InvalidOperationException("Ya existe una reserva confirmada en ese rango de fechas");

            var previousStatus = reserva.Status;
            reserva.Status = "confirmed";

            _context.ReservationStatusHistory.Add(new ReservationStatusHistory
            {
                ReservaId = reserva.Id,
                PreviousStatus = previousStatus,
                NewStatus = "confirmed"
            });

            await _context.SaveChangesAsync();

            return await GetByIdInternalAsync(reserva.Id);
        }
        catch (Exception ex) when (ex is not ArgumentException and not KeyNotFoundException
            and not UnauthorizedAccessException and not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al confirmar la reserva", ex);
        }
    }

    public async Task<ReservaResponseDTO> CancelAsync(Guid userId, Guid reservaId)
    {
        try
        {
            var reserva = await _context.Reservas
                .Include(r => r.Inmueble)
                .FirstOrDefaultAsync(r => r.Id == reservaId)
                ?? throw new KeyNotFoundException("Reserva no encontrada");

            var isGuest = reserva.GuestId == userId;
            var isOwner = reserva.Inmueble.OwnerId == userId;

            if (!isGuest && !isOwner)
                throw new UnauthorizedAccessException("No tienes permiso para cancelar esta reserva");

            if (reserva.Status == "cancelled" || reserva.Status == "completed")
                throw new ArgumentException("No se puede cancelar una reserva en estado " + reserva.Status);

            var previousStatus = reserva.Status;
            reserva.Status = "cancelled";

            _context.ReservationStatusHistory.Add(new ReservationStatusHistory
            {
                ReservaId = reserva.Id,
                PreviousStatus = previousStatus,
                NewStatus = "cancelled"
            });

            await _context.SaveChangesAsync();

            return await GetByIdInternalAsync(reserva.Id);
        }
        catch (Exception ex) when (ex is not ArgumentException and not KeyNotFoundException
            and not UnauthorizedAccessException)
        {
            throw new InvalidOperationException("Error al cancelar la reserva", ex);
        }
    }

    public async Task<List<ReservaListResponseDTO>> ListByUserAsync(Guid userId, string role)
    {
        try
        {
            var query = _context.Reservas
                .Include(r => r.Inmueble)
                .AsQueryable();

            if (role == "guest")
                query = query.Where(r => r.GuestId == userId);
            else
                query = query.Where(r => r.Inmueble.OwnerId == userId);

            var reservas = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reservas.Select(r => new ReservaListResponseDTO
            {
                Id = r.Id,
                InmuebleTitle = r.Inmueble.Title,
                CheckIn = r.CheckIn,
                CheckOut = r.CheckOut,
                TotalPrice = r.TotalPrice,
                Status = r.Status
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al listar reservas", ex);
        }
    }

    public async Task<ReservaResponseDTO> GetByIdAsync(Guid userId, Guid reservaId)
    {
        try
        {
            var reserva = await _context.Reservas
                .Include(r => r.Inmueble)
                .Include(r => r.Guest)
                .FirstOrDefaultAsync(r => r.Id == reservaId)
                ?? throw new KeyNotFoundException("Reserva no encontrada");

            var isGuest = reserva.GuestId == userId;
            var isOwner = reserva.Inmueble.OwnerId == userId;

            if (!isGuest && !isOwner)
                throw new UnauthorizedAccessException("No tienes permiso para ver esta reserva");

            return MapToResponse(reserva);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException and not UnauthorizedAccessException)
        {
            throw new InvalidOperationException("Error al obtener la reserva", ex);
        }
    }

    private async Task<ReservaResponseDTO> GetByIdInternalAsync(Guid reservaId)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Inmueble)
            .Include(r => r.Guest)
            .FirstAsync(r => r.Id == reservaId);

        return MapToResponse(reserva);
    }

    private static ReservaResponseDTO MapToResponse(Reserva reserva)
    {
        return new ReservaResponseDTO
        {
            Id = reserva.Id,
            InmuebleId = reserva.InmuebleId,
            InmuebleTitle = reserva.Inmueble.Title,
            GuestId = reserva.GuestId,
            GuestName = reserva.Guest.FullName,
            CheckIn = reserva.CheckIn,
            CheckOut = reserva.CheckOut,
            CheckInTime = reserva.CheckInTime,
            CheckOutTime = reserva.CheckOutTime,
            TotalPrice = reserva.TotalPrice,
            Status = reserva.Status,
            CreatedAt = reserva.CreatedAt
        };
    }
}
