using Microsoft.EntityFrameworkCore;
using RentasCortas.Common.Exceptions;
using RentasCortas.Common.Notifications;
using RentasCortas.Data;
using RentasCortas.Models;

namespace RentasCortas.Features.Reservas;

public class ReservasService : IReservasService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public ReservasService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<ReservaResponseDTO> CreateAsync(Guid guestId, CreateReservaDTO dto)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (dto.CheckIn < today)
                throw new ArgumentException("La fecha de check-in no puede ser en el pasado");

            if (dto.CheckIn < today.AddDays(2))
                throw new ArgumentException("La reserva debe hacerse con al menos 2 días de antelación");

            if (dto.CheckOut <= dto.CheckIn)
                throw new ArgumentException("La fecha de check-out debe ser posterior al check-in");

            if (dto.CheckOut.DayNumber - dto.CheckIn.DayNumber > 5)
                throw new ArgumentException("La estadía máxima permitida es de 5 noches");

            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == dto.InmuebleId && i.Status == "active")
                ?? throw new KeyNotFoundException($"El inmueble con id {dto.InmuebleId} no existe o no está disponible");

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
                ?? throw new KeyNotFoundException($"La reserva con id {reservaId} no existe");

            if (reserva.Inmueble.OwnerId != ownerId)
                throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");

            if (reserva.Status != "pending")
                throw new ArgumentException($"Solo se pueden confirmar reservas en estado pending. Estado actual: {reserva.Status}");

            // KYC obligatorio solo para la primera reserva del guest
            var hasCompletedReservations = await _context.Reservas
                .AnyAsync(r => r.GuestId == reserva.GuestId
                    && r.Id != reservaId
                    && (r.Status == "confirmed" || r.Status == "completed"));

            if (!hasCompletedReservations && reserva.Guest.KycStatus != "approved")
                throw new ArgumentException("Debes completar la validación de identidad antes de realizar tu primera reserva");

            // Anti double-booking
            var hasConflict = await _context.Reservas
                .AnyAsync(r => r.InmuebleId == reserva.InmuebleId
                    && r.Id != reservaId
                    && r.Status == "confirmed"
                    && r.CheckIn < reserva.CheckOut
                    && r.CheckOut > reserva.CheckIn);

            if (hasConflict)
                throw new ConflictException("Ya existe una reserva confirmada para este inmueble en las fechas seleccionadas");

            var previousStatus = reserva.Status;
            reserva.Status = "confirmed";

            _context.ReservationStatusHistory.Add(new ReservationStatusHistory
            {
                ReservaId = reserva.Id,
                PreviousStatus = previousStatus,
                NewStatus = "confirmed"
            });

            await _context.SaveChangesAsync();

            var confirmMessage = $"Tu reserva en \"{reserva.Inmueble.Title}\" del {reserva.CheckIn} al {reserva.CheckOut} ha sido confirmada. Check-in: 14:00, Check-out: 12:00.";

            await _notificationService.SendInAppAsync(reserva.GuestId, "reservation_confirmed", confirmMessage);
            await _notificationService.SendEmailAsync(reserva.GuestId, "Reserva Confirmada", confirmMessage);

            return await GetByIdInternalAsync(reserva.Id);
        }
        catch (Exception ex) when (ex is not ArgumentException and not KeyNotFoundException
            and not UnauthorizedAccessException and not ConflictException)
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
                ?? throw new KeyNotFoundException($"La reserva con id {reservaId} no existe");

            var isGuest = reserva.GuestId == userId;
            var isOwner = reserva.Inmueble.OwnerId == userId;

            if (!isGuest && !isOwner)
                throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");

            if (reserva.Status == "cancelled" || reserva.Status == "completed")
                throw new ArgumentException($"No se puede cancelar una reserva en estado {reserva.Status}");

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
                ?? throw new KeyNotFoundException($"La reserva con id {reservaId} no existe");

            var isGuest = reserva.GuestId == userId;
            var isOwner = reserva.Inmueble.OwnerId == userId;

            if (!isGuest && !isOwner)
                throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");

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
