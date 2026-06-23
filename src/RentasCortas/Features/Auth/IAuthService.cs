namespace RentasCortas.Features.Auth;

public interface IAuthService
{
    Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
    Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
}