using backend.Data;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Utils
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                // Verifica conexão
                if (!await context.Database.CanConnectAsync())
                {
                    Console.WriteLine("❌ Não foi possível conectar ao banco de dados");
                    return;
                }

                Console.WriteLine("✅ Conectado ao banco de dados");

                // Criar perfis se não existirem
                if (!await context.Perfis.AnyAsync())
                {
                    Console.WriteLine("📝 Criando perfis padrão...");
                    
                    var perfis = new List<Perfil>
                    {
                        new Perfil 
                        { 
                            Nome = "Administrador", 
                            Descricao = "Acesso total ao sistema"
                        },
                        new Perfil 
                        { 
                            Nome = "Gestor", 
                            Descricao = "Gerencia equipes e visualiza dashboards"
                        },
                        new Perfil 
                        { 
                            Nome = "Colaborador", 
                            Descricao = "Usuário padrão do sistema"
                        }
                    };

                    context.Perfis.AddRange(perfis);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Perfis criados com sucesso");
                }

                // Criar usuário admin se não existir
                if (!await context.Usuarios.AnyAsync(u => u.Email == "admin@vivo.com.br"))
                {
                    Console.WriteLine("👤 Criando usuário administrador...");
                    
                    var perfilAdmin = await context.Perfis.FirstAsync(p => p.Nome == "Administrador");
                    Console.WriteLine($"📝 Perfil Admin encontrado: ID {perfilAdmin.Id}");
                    
                    var admin = new Usuario
                    {
                        Email = "admin@vivo.com.br",
                        Senha = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        NomeCompleto = "Administrador Sistema",
                        PerfilId = perfilAdmin.Id,
                        Ativo = true,
                        PrimeiroAcesso = false,
                        Departamento = "TI",
                        Cargo = "Administrador"
                    };

                    Console.WriteLine($"🔐 Hash da senha gerado com {admin.Senha.Length} caracteres");
                    context.Usuarios.Add(admin);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Usuário admin criado: admin@vivo.com.br / Admin@123");
                }
                else
                {
                    Console.WriteLine("ℹ️ Usuário admin já existe, pulando criação");
                }

                // Criar usuários de teste
                var usuariosTeste = new[]
                {
                    ("joao.silva@vivo.com.br", "João Silva", "Desenvolvimento", "Desenvolvedor Backend"),
                    ("maria.oliveira@vivo.com.br", "Maria Oliveira", "Dados", "Analista de Dados"),
                    ("carlos.santos@vivo.com.br", "Carlos Santos", "Frontend", "Desenvolvedor Frontend")
                };

                var perfilColaborador = await context.Perfis.FirstAsync(p => p.Nome == "Colaborador");
                var adminUser = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@vivo.com.br");
                
                if (adminUser == null)
                {
                    Console.WriteLine("⚠️ Usuário admin não foi encontrado após criação");
                    return;
                }

                foreach (var (email, nome, depto, cargo) in usuariosTeste)
                {
                    if (!await context.Usuarios.AnyAsync(u => u.Email == email))
                    {
                        var usuario = new Usuario
                        {
                            Email = email,
                            Senha = BCrypt.Net.BCrypt.HashPassword("Senha@123"),
                            NomeCompleto = nome,
                            PerfilId = perfilColaborador.Id,
                            Ativo = true,
                            PrimeiroAcesso = true,
                            Departamento = depto,
                            Cargo = cargo,
                            GestorId = adminUser.Id,
                            DataAdmissao = DateTime.Now.AddDays(-30)
                        };

                        context.Usuarios.Add(usuario);
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Usuários de teste criados");
                
                // Listar usuários criados
                Console.WriteLine("\n📋 Usuários disponíveis para login:");
                Console.WriteLine("====================================");
                var usuarios = await context.Usuarios.Include(u => u.Perfil).ToListAsync();
                foreach (var u in usuarios)
                {
                    Console.WriteLine($"📧 {u.Email} | Perfil: {u.Perfil?.Nome} | Senha: " + 
                                    (u.Email.Contains("admin") ? "Admin@123" : "Senha@123"));
                }
                Console.WriteLine("====================================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao popular banco: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Detalhes: {ex.InnerException.Message}");
                }
            }
        }
    }
}