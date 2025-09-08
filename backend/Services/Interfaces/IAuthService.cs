using backend.Models.DTOs;

namespace backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null, string? userAgent = null);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string token);
        Task<bool> EnviarEmailRecuperacaoSenhaAsync(string email);
    }
}