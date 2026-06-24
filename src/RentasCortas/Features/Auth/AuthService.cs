using Microsoft.EntityFrameworkCore;
using RentasCortas.Common.Notifications;
using RentasCortas.Common.Security;
using RentasCortas.Data;
using RentasCortas.Models;

namespace RentasCortas.Features.Auth;

public class AuthService : IAuthService
{
    private static readonly string[] ValidRoles = ["guest", "owner"];

    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly INotificationService _notificationService;

    public AuthService(AppDbContext context, JwtHelper jwtHelper, INotificationService notificationService)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _notificationService = notificationService;
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
    {
        try
        {
            if (!ValidRoles.Contains(dto.Role))
                throw new ArgumentException("El rol debe ser 'guest' o 'owner'");

            var emailExists = await _context.Usuarios
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
                throw new ArgumentException("El email ya está registrado");

            var usuario = new Usuario
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Phone = dto.Phone,
                Role = dto.Role
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(usuario.Id, usuario.Email, usuario.Role);

            await _notificationService.SendEmailAsync(
                usuario.Id,
                "Bienvenido a RentasCortas",
                $"Hola {usuario.FullName}, tu cuenta ha sido creada exitosamente. ¡Bienvenido a RentasCortas!");

            return new AuthResponseDTO
            {
                Id = usuario.Id,
                Email = usuario.Email,
                FullName = usuario.FullName,
                Role = usuario.Role,
                Token = token
            };
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al registrar el usuario", ex);
        }
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email)
                ?? throw new KeyNotFoundException("Credenciales inválidas");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
                throw new KeyNotFoundException("Credenciales inválidas");

            var token = _jwtHelper.GenerateToken(usuario.Id, usuario.Email, usuario.Role);

            return new AuthResponseDTO
            {
                Id = usuario.Id,
                Email = usuario.Email,
                FullName = usuario.FullName,
                Role = usuario.Role,
                Token = token
            };
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al iniciar sesión", ex);
        }
    }
}
