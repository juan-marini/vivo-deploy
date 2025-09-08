using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    var userCount = await _context.Usuarios.CountAsync();
                    var perfilCount = await _context.Perfis.CountAsync();

                    return Ok(new
                    {
                        status = "✅ Conectado",
                        database = "vivo_knowledge_db",
                        usuarios = userCount,
                        perfis = perfilCount,
                        timestamp = DateTime.Now
                    });
                }

                return StatusCode(500, new { status = "❌ Sem conexão" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "❌ Erro",
                    message = ex.Message
                });
            }
        }

        [HttpGet("hash-test")]
        public async Task<IActionResult> TestPasswordHash()
        {
            try
            {
                var email = "admin@vivo.com.br";
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuário admin não encontrado" });
                }

                var senhaCorreta = "Admin@123";
                var verificacao = BCrypt.Net.BCrypt.Verify(senhaCorreta, usuario.Senha);

                return Ok(new
                {
                    email = usuario.Email,
                    senhaEsperada = senhaCorreta,
                    hashArmazenado = usuario.Senha.Substring(0, 20) + "...", // Ocultar parte do hash
                    verificacaoOk = verificacao,
                    tamanhoHash = usuario.Senha?.Length ?? 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("fix-admin")]
        public async Task<IActionResult> FixAdminPassword()
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@vivo.com.br");
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuário admin não encontrado" });
                }

                // Gerar novo hash correto
                var novoHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                usuario.Senha = novoHash;
                
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Senha do admin atualizada com sucesso",
                    novoHashTamanho = novoHash.Length,
                    verificacao = BCrypt.Net.BCrypt.Verify("Admin@123", novoHash)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}