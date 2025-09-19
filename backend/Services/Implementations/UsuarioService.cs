using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models.DTOs;
using backend.Models.Entities;
using backend.Services.Interfaces;
using MemberDetailedProgressDto = backend.Models.DTOs.MemberDetailedProgressDto;

namespace backend.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(ApplicationDbContext context, ILogger<UsuarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.Ativo)
                    .Include(u => u.Perfil)
                    .ToListAsync();

                var teamMembers = new List<TeamMemberDto>();

                foreach (var usuario in usuarios)
                {
                    var progress = await CalculateUserProgressAsync(usuario.Email);
                    var status = GetStatusFromProgress(progress);

                    teamMembers.Add(new TeamMemberDto
                    {
                        Id = usuario.Email,
                        Name = usuario.NomeCompleto,
                        Role = usuario.Cargo ?? "Colaborador",
                        StartDate = usuario.DataAdmissao?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy"),
                        Progress = progress,
                        Status = status,
                        AvatarUrl = usuario.AvatarUrl,
                        Department = usuario.Departamento,
                        Phone = usuario.Telefone,
                        AdmissionDate = usuario.DataAdmissao,
                        LastLogin = usuario.UltimoLogin,
                        IsActive = usuario.Ativo,
                        Team = usuario.Perfil?.Nome ?? "Sem Equipe"
                    });
                }

                return teamMembers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar membros da equipe");
                throw;
            }
        }

        public async Task<UsuarioDto?> GetUsuarioByIdAsync(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                    return null;

                return new UsuarioDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    NomeCompleto = usuario.NomeCompleto,
                    PerfilId = usuario.PerfilId,
                    AvatarUrl = usuario.AvatarUrl,
                    Ativo = usuario.Ativo,
                    PrimeiroAcesso = usuario.PrimeiroAcesso,
                    DataAdmissao = usuario.DataAdmissao,
                    Telefone = usuario.Telefone,
                    Departamento = usuario.Departamento,
                    Cargo = usuario.Cargo,
                    GestorId = usuario.GestorId,
                    UltimoLogin = usuario.UltimoLogin
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por ID {UserId}", id);
                throw;
            }
        }

        public async Task<UsuarioDto?> GetUsuarioByEmailAsync(string email)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (usuario == null)
                    return null;

                return new UsuarioDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    NomeCompleto = usuario.NomeCompleto,
                    PerfilId = usuario.PerfilId,
                    AvatarUrl = usuario.AvatarUrl,
                    Ativo = usuario.Ativo,
                    PrimeiroAcesso = usuario.PrimeiroAcesso,
                    DataAdmissao = usuario.DataAdmissao,
                    Telefone = usuario.Telefone,
                    Departamento = usuario.Departamento,
                    Cargo = usuario.Cargo,
                    GestorId = usuario.GestorId,
                    UltimoLogin = usuario.UltimoLogin
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por email {Email}", email);
                throw;
            }
        }

        private async Task<int> CalculateUserProgressAsync(string userId)
        {
            try
            {
                var totalTopics = await _context.Topics.CountAsync();
                if (totalTopics == 0) return 0;

                var completedTopics = await _context.MemberProgresses
                    .Where(mp => mp.UserId == userId && mp.IsCompleted)
                    .CountAsync();

                return (int)Math.Round((double)completedTopics / totalTopics * 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular progresso do usuário {UserId}", userId);
                return 0;
            }
        }

        private string GetStatusFromProgress(int progress)
        {
            return progress switch
            {
                0 => "Não iniciado",
                < 30 => "Iniciando",
                < 70 => "Em progresso",
                < 100 => "Quase concluído",
                _ => "Concluído"
            };
        }
    }
}