using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using MemberDetailedProgressDto = backend.Models.DTOs.MemberDetailedProgressDto;
using backend.Services.Interfaces;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpGet("team-members")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetTeamMembers()
        {
            try
            {
                var teamMembers = await _usuarioService.GetTeamMembersAsync();
                return Ok(teamMembers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar membros da equipe");
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound(new { success = false, message = "Usuário não encontrado" });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário {UserId}", id);
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuarioByEmail(string email)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByEmailAsync(email);
                if (usuario == null)
                {
                    return NotFound(new { success = false, message = "Usuário não encontrado" });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por email {Email}", email);
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }
    }
}