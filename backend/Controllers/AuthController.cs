using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using backend.Services.Interfaces;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Dados inválidos"
                    });
                }

                var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers.UserAgent.ToString();

                var result = await _authService.LoginAsync(request, ipAddress, userAgent);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de login");
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = "Erro interno do servidor"
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Token de renovação é obrigatório"
                    });
                }

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de refresh token");
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = "Erro interno do servidor"
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                await _authService.LogoutAsync(token);
                return Ok(new { success = true, message = "Logout realizado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de logout");
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        [HttpPost("esqueci-senha")]
        public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Email é obrigatório" });
                }

                var result = await _authService.EnviarEmailRecuperacaoSenhaAsync(request.Email);
                
                return Ok(new
                {
                    success = true,
                    message = "Se o email estiver cadastrado, você receberá instruções para recuperação da senha"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de esqueci senha");
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var nomeCompleto = User.FindFirst(ClaimTypes.Name)?.Value;
                var perfil = User.FindFirst("perfil")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Token inválido" });
                }

                return Ok(new
                {
                    success = true,
                    usuario = new
                    {
                        id = int.Parse(userId),
                        email = email,
                        nomeCompleto = nomeCompleto,
                        perfil = perfil
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuário atual");
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        [HttpPost("verificar-token")]
        [Authorize]
        public IActionResult VerificarToken()
        {
            return Ok(new { success = true, message = "Token válido" });
        }
    }
}