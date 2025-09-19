using backend.Models.DTOs;
using MemberDetailedProgressDto = backend.Models.DTOs.MemberDetailedProgressDto;

namespace backend.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync();
        Task<UsuarioDto?> GetUsuarioByIdAsync(int id);
        Task<UsuarioDto?> GetUsuarioByEmailAsync(string email);
    }
}