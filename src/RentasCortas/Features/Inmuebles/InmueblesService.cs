using Microsoft.EntityFrameworkCore;
using RentasCortas.Common.Storage;
using RentasCortas.Data;
using RentasCortas.Models;

namespace RentasCortas.Features.Inmuebles;

public class InmueblesService : IInmueblesService
{
    private static readonly string[] ValidStatuses = ["active", "inactive"];

    private readonly AppDbContext _context;
    private readonly IFileStorageService _storage;

    public InmueblesService(AppDbContext context, IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<InmuebleResponseDTO> CreateAsync(Guid ownerId, CreateInmuebleDTO dto)
    {
        try
        {
            var inmueble = new Inmueble
            {
                OwnerId = ownerId,
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                PricePerNight = dto.PricePerNight
            };

            _context.Inmuebles.Add(inmueble);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(inmueble.Id);
        }
        catch (Exception ex) when (ex is not ArgumentException and not KeyNotFoundException)
        {
            throw new InvalidOperationException("Error al crear el inmueble", ex);
        }
    }

    public async Task<InmuebleResponseDTO> UpdateAsync(Guid ownerId, Guid inmuebleId, UpdateInmuebleDTO dto)
    {
        try
        {
            var inmueble = await GetOwnedInmuebleAsync(ownerId, inmuebleId);

            if (!ValidStatuses.Contains(dto.Status))
                throw new ArgumentException("El status debe ser 'active' o 'inactive'");

            inmueble.Title = dto.Title;
            inmueble.Description = dto.Description;
            inmueble.Location = dto.Location;
            inmueble.PricePerNight = dto.PricePerNight;
            inmueble.Status = dto.Status;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(inmueble.Id);
        }
        catch (Exception ex) when (ex is not ArgumentException and not KeyNotFoundException and not UnauthorizedAccessException)
        {
            throw new InvalidOperationException("Error al actualizar el inmueble", ex);
        }
    }

    public async Task SoftDeleteAsync(Guid ownerId, Guid inmuebleId)
    {
        try
        {
            var inmueble = await GetOwnedInmuebleAsync(ownerId, inmuebleId);

            inmueble.Status = "deleted";
            inmueble.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException and not UnauthorizedAccessException)
        {
            throw new InvalidOperationException("Error al eliminar el inmueble", ex);
        }
    }

    public async Task<InmuebleResponseDTO> GetByIdAsync(Guid inmuebleId)
    {
        try
        {
            var inmueble = await _context.Inmuebles
                .Include(i => i.Owner)
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.Id == inmuebleId)
                ?? throw new KeyNotFoundException("Inmueble no encontrado");

            return MapToResponse(inmueble);
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException("Error al obtener el inmueble", ex);
        }
    }

    public async Task<List<InmuebleListResponseDTO>> ListAsync(InmuebleFilterDTO? filter)
    {
        try
        {
            var query = _context.Inmuebles
                .Include(i => i.Images)
                .Where(i => i.Status == "active")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter?.Location))
                query = query.Where(i => i.Location.ToLower().Contains(filter.Location.ToLower()));

            if (filter?.AvailableFrom.HasValue == true && filter?.AvailableTo.HasValue == true)
            {
                var from = filter.AvailableFrom.Value;
                var to = filter.AvailableTo.Value;

                // Excluir inmuebles que tienen reservas confirmadas que se solapan con el rango
                query = query.Where(i => !_context.Reservas
                    .Any(r => r.InmuebleId == i.Id
                        && r.Status == "confirmed"
                        && r.CheckIn < to
                        && r.CheckOut > from));
            }

            var inmuebles = await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return inmuebles.Select(i => new InmuebleListResponseDTO
            {
                Id = i.Id,
                Title = i.Title,
                Location = i.Location,
                PricePerNight = i.PricePerNight,
                Status = i.Status,
                MainImage = i.Images.OrderBy(img => img.CreatedAt).FirstOrDefault()?.ImageUrl
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al listar inmuebles", ex);
        }
    }

    public async Task<ImageResponseDTO> AddImageAsync(Guid ownerId, Guid inmuebleId, IFormFile file)
    {
        try
        {
            await GetOwnedInmuebleAsync(ownerId, inmuebleId);

            var imageUrl = await _storage.SaveFileAsync(file, inmuebleId.ToString());

            var image = new PropertyImage
            {
                InmuebleId = inmuebleId,
                ImageUrl = imageUrl
            };

            _context.PropertyImages.Add(image);
            await _context.SaveChangesAsync();

            return new ImageResponseDTO
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                CreatedAt = image.CreatedAt
            };
        }
        catch (Exception ex) when (ex is not KeyNotFoundException and not UnauthorizedAccessException)
        {
            throw new InvalidOperationException("Error al subir la imagen", ex);
        }
    }

    public async Task RemoveImageAsync(Guid ownerId, Guid inmuebleId, Guid imageId)
    {
        try
        {
            await GetOwnedInmuebleAsync(ownerId, inmuebleId);

            var image = await _context.PropertyImages
                .FirstOrDefaultAsync(img => img.Id == imageId && img.InmuebleId == inmuebleId)
                ?? throw new KeyNotFoundException("Imagen no encontrada");

            _storage.DeleteFile(image.ImageUrl);
            _context.PropertyImages.Remove(image);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException and not UnauthorizedAccessException)
        {
            throw new InvalidOperationException("Error al eliminar la imagen", ex);
        }
    }

    private async Task<Inmueble> GetOwnedInmuebleAsync(Guid ownerId, Guid inmuebleId)
    {
        var inmueble = await _context.Inmuebles
            .FirstOrDefaultAsync(i => i.Id == inmuebleId)
            ?? throw new KeyNotFoundException("Inmueble no encontrado");

        if (inmueble.OwnerId != ownerId)
            throw new UnauthorizedAccessException("No tienes permiso para modificar este inmueble");

        return inmueble;
    }

    private static InmuebleResponseDTO MapToResponse(Inmueble inmueble)
    {
        return new InmuebleResponseDTO
        {
            Id = inmueble.Id,
            OwnerId = inmueble.OwnerId,
            OwnerName = inmueble.Owner.FullName,
            Title = inmueble.Title,
            Description = inmueble.Description,
            Location = inmueble.Location,
            PricePerNight = inmueble.PricePerNight,
            Status = inmueble.Status,
            CreatedAt = inmueble.CreatedAt,
            Images = inmueble.Images
                .OrderBy(img => img.CreatedAt)
                .Select(img => new ImageResponseDTO
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    CreatedAt = img.CreatedAt
                }).ToList()
        };
    }
}
