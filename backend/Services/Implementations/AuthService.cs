using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models.DTOs;
using backend.Models.Entities;
using backend.Services.Interfaces;

namespace backend.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context, 
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                // Buscar usuário com perfil
                var usuario = await _context.Usuarios
                    .Include(u => u.Perfil)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (usuario == null)
                {
                    return new LoginResponseDto 
                    { 
                        Success = false, 
                        Message = "Email ou senha inválidos" 
                    };
                }

                // Verificar se usuário está ativo
                if (!usuario.Ativo)
                {
                    return new LoginResponseDto 
                    { 
                        Success = false, 
                        Message = "Usuário inativo. Entre em contato com o administrador." 
                    };
                }

                // Verificar senha
                if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
                {
                    _logger.LogWarning($"Tentativa de login falhada para o email: {request.Email}");
                    return new LoginResponseDto 
                    { 
                        Success = false, 
                        Message = "Email ou senha inválidos" 
                    };
                }

                // Gerar tokens
                var token = _tokenService.GenerateToken(usuario);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddHours(2);

                // Limpar sessões antigas do usuário
                var sessoesAntigas = await _context.Sessoes
                    .Where(s => s.UsuarioId == usuario.Id)
                    .ToListAsync();
                _context.Sessoes.RemoveRange(sessoesAntigas);

                // Criar nova sessão
                var sessao = new Sessao
                {
                    UsuarioId = usuario.Id,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiraEm = expiresAt,
                    IpAddress = ipAddress ?? "Unknown",
                    UserAgent = userAgent ?? "Unknown",
                    CriadoEm = DateTime.UtcNow
                };

                _context.Sessoes.Add(sessao);

                // Atualizar último login
                usuario.UltimoLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Login realizado com sucesso para: {usuario.Email}");

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Login realizado com sucesso",
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    Usuario = new UsuarioDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NomeCompleto = usuario.NomeCompleto,
                        Perfil = usuario.Perfil?.Nome ?? "Colaborador",
                        AvatarUrl = usuario.AvatarUrl,
                        PrimeiroAcesso = usuario.PrimeiroAcesso,
                        Departamento = usuario.Departamento,
                        Cargo = usuario.Cargo
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar login");
                return new LoginResponseDto 
                { 
                    Success = false, 
                    Message = "Erro ao processar login. Tente novamente." 
                };
            }
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var sessao = await _context.Sessoes
                    .Include(s => s.Usuario)
                    .ThenInclude(u => u!.Perfil)
                    .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);

                if (sessao == null || sessao.ExpiraEm < DateTime.UtcNow)
                {
                    return new LoginResponseDto 
                    { 
                        Success = false, 
                        Message = "Token inválido ou expirado" 
                    };
                }

                var usuario = sessao.Usuario!;
                var newToken = _tokenService.GenerateToken(usuario);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddHours(2);

                // Atualizar sessão
                sessao.Token = newToken;
                sessao.RefreshToken = newRefreshToken;
                sessao.ExpiraEm = expiresAt;
                
                await _context.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Token renovado com sucesso",
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renovar token");
                return new LoginResponseDto 
                { 
                    Success = false, 
                    Message = "Erro ao renovar token" 
                };
            }
        }

        public async Task LogoutAsync(string token)
        {
            try
            {
                var sessao = await _context.Sessoes
                    .FirstOrDefaultAsync(s => s.Token == token);

                if (sessao != null)
                {
                    _context.Sessoes.Remove(sessao);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar logout");
            }
        }

        public async Task<bool> EnviarEmailRecuperacaoSenhaAsync(string email)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (usuario == null)
                {
                    // Não revelar se o email existe ou não
                    return true;
                }

                // TODO: Implementar envio de email
                _logger.LogInformation($"Email de recuperação solicitado para: {email}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de recuperação");
                return false;
            }
        }
    }
}