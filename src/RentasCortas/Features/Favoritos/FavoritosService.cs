using Microsoft.EntityFrameworkCore;
using RentasCortas.Data;
using RentasCortas.Models;

namespace RentasCortas.Features.Favoritos;

public class FavoritosService : IFavoritosService
{
    private readonly AppDbContext _context;

    public FavoritosService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FavoritoResponseDTO> AddAsync(Guid guestId, Guid inmuebleId)
    {
        try
        {
            var inmueble = await _context.Inmuebles
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.Id == inmuebleId && i.Status == "active")
                ?? throw new KeyNotFoundException($"El inmueble con id {inmuebleId} no existe o no está disponible");

            var exists = await _context.Favoritos
                .AnyAsync(f => f.GuestId == guestId && f.InmuebleId == inmuebleId);

            if (exists)
                throw new InvalidOperationException("Este inmueble ya se encuentra en tus favoritos");

            var favorito = new Favorito
            {
                GuestId = guestId,
                InmuebleId = inmuebleId
            };

            _context.Favoritos.Add(favorito);
            await _context.SaveChangesAsync();

            return MapToDTO(favorito, inmueble);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException and not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al agregar el inmueble a favoritos", ex);
        }
    }

    public async Task RemoveAsync(Guid guestId, Guid inmuebleId)
    {
        try
        {
            var favorito = await _context.Favoritos
                .FirstOrDefaultAsync(f => f.GuestId == guestId && f.InmuebleId == inmuebleId)
                ?? throw new KeyNotFoundException("Este inmueble no se encuentra en tus favoritos");

            _context.Favoritos.Remove(favorito);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException("Error al eliminar el inmueble de favoritos", ex);
        }
    }

    public async Task<List<FavoritoResponseDTO>> ListByGuestAsync(Guid guestId)
    {
        try
        {
            var favoritos = await _context.Favoritos
                .Where(f => f.GuestId == guestId)
                .Include(f => f.Inmueble)
                    .ThenInclude(i => i.Images)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return favoritos.Select(f => MapToDTO(f, f.Inmueble)).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al obtener los favoritos", ex);
        }
    }

    private static FavoritoResponseDTO MapToDTO(Favorito favorito, Inmueble inmueble)
    {
        return new FavoritoResponseDTO
        {
            Id = favorito.Id,
            InmuebleId = inmueble.Id,
            Title = inmueble.Title,
            Location = inmueble.Location,
            PricePerNight = inmueble.PricePerNight,
            Images = inmueble.Images.Select(img => img.ImageUrl).ToList(),
            CreatedAt = favorito.CreatedAt
        };
    }
}