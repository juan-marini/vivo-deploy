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

                // Criar ou atualizar usuário admin
                var adminUser = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@vivo.com.br");
                if (adminUser == null)
                {
                    Console.WriteLine("👤 Criando usuário administrador...");
                    
                    var perfilAdmin = await context.Perfis.FirstAsync(p => p.Nome == "Administrador");
                    Console.WriteLine($"📝 Perfil Admin encontrado: ID {perfilAdmin.Id}");
                    
                    adminUser = new Usuario
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

                    Console.WriteLine($"🔐 Hash da senha gerado com {adminUser.Senha.Length} caracteres");
                    context.Usuarios.Add(adminUser);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Usuário admin criado: admin@vivo.com.br / Admin@123");
                }
                else
                {
                    // Verificar se o hash da senha está correto
                    if (adminUser.Senha.Length < 50) // BCrypt hash deve ter ~60 caracteres
                    {
                        Console.WriteLine("🔧 Corrigindo hash da senha do admin...");
                        adminUser.Senha = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                        await context.SaveChangesAsync();
                        Console.WriteLine("✅ Hash da senha do admin corrigido");
                    }
                    else
                    {
                        Console.WriteLine("ℹ️ Usuário admin já existe com hash válido");
                    }
                }

                // Criar usuários de teste
                var usuariosTeste = new[]
                {
                    ("joao.silva@vivo.com.br", "João Silva", "Desenvolvimento", "Desenvolvedor Backend"),
                    ("maria.oliveira@vivo.com.br", "Maria Oliveira", "Dados", "Analista de Dados"),
                    ("carlos.santos@vivo.com.br", "Carlos Santos", "Frontend", "Desenvolvedor Frontend")
                };

                // Criar usuário gestor
                var perfilGestor = await context.Perfis.FirstAsync(p => p.Nome == "Gestor");
                if (!await context.Usuarios.AnyAsync(u => u.Email == "gestor@vivo.com.br"))
                {
                    var gestorUser = new Usuario
                    {
                        Email = "gestor@vivo.com.br",
                        Senha = BCrypt.Net.BCrypt.HashPassword("Gestor@123"),
                        NomeCompleto = "Ana Gestora",
                        PerfilId = perfilGestor.Id,
                        Ativo = true,
                        PrimeiroAcesso = false,
                        Departamento = "Gestão",
                        Cargo = "Gerente de Equipe",
                        DataAdmissao = DateTime.Now.AddDays(-60)
                    };

                    context.Usuarios.Add(gestorUser);
                    Console.WriteLine("✅ Usuário gestor criado: gestor@vivo.com.br / Gestor@123");
                }

                var perfilColaborador = await context.Perfis.FirstAsync(p => p.Nome == "Colaborador");
                var adminUserRef = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@vivo.com.br");
                
                if (adminUserRef == null)
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
                            GestorId = adminUserRef.Id,
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
                    string senha = u.Email.Contains("admin") ? "Admin@123" :
                                  u.Email.Contains("gestor") ? "Gestor@123" : "Senha@123";
                    Console.WriteLine($"📧 {u.Email} | Perfil: {u.Perfil?.Nome} | Senha: {senha}");
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