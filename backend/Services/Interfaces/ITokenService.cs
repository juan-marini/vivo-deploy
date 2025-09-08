using backend.Models.Entities;

namespace backend.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(Usuario usuario);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
    }
}