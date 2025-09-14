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

                // Seed Topics
                await SeedTopics(context);

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

        private static async Task SeedTopics(ApplicationDbContext context)
        {
            if (!await context.Topics.AnyAsync())
            {
                Console.WriteLine("📚 Criando tópicos padrão...");

                var topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "SQL Server",
                        Description = "Banco de dados principal utilizado para armazenar dados de clientes e transações. Aprenda sobre configuração, otimização e melhores práticas.",
                        Category = "Banco de Dados",
                        EstimatedTime = "2h",
                        IsActive = true
                    },
                    new Topic
                    {
                        Title = "Oracle",
                        Description = "Banco de dados secundário utilizado para sistemas específicos e data warehouse.",
                        Category = "Banco de Dados",
                        EstimatedTime = "1.5h",
                        IsActive = true
                    },
                    new Topic
                    {
                        Title = "MongoDB",
                        Description = "Banco de dados NoSQL para projetos específicos",
                        Category = "Banco de Dados",
                        EstimatedTime = "3h",
                        IsActive = true
                    },
                    new Topic
                    {
                        Title = "Angular",
                        Description = "Framework frontend utilizado para desenvolvimento de SPAs",
                        Category = "Frontend",
                        EstimatedTime = "4h",
                        IsActive = true
                    },
                    new Topic
                    {
                        Title = "React",
                        Description = "Biblioteca JavaScript para construção de interfaces de usuário",
                        Category = "Frontend",
                        EstimatedTime = "3.5h",
                        IsActive = true
                    },
                    new Topic
                    {
                        Title = "ASP.NET Core",
                        Description = "Framework backend para desenvolvimento de APIs REST",
                        Category = "Backend",
                        EstimatedTime = "5h",
                        IsActive = true
                    },
                    new Topic
                    {
                        Title = "Docker",
                        Description = "Containerização de aplicações para deployment",
                        Category = "DevOps",
                        EstimatedTime = "2.5h",
                        IsActive = true
                    },
                    new Topic
                    {
                        Title = "Kubernetes",
                        Description = "Orquestração de containers em produção",
                        Category = "DevOps",
                        EstimatedTime = "6h",
                        IsActive = true
                    },
                    new Topic
                    {
                        Title = "Power BI",
                        Description = "Ferramenta de Business Intelligence para análise de dados",
                        Category = "Análise de Dados",
                        EstimatedTime = "3h",
                        IsActive = true
                    },
                    new Topic
                    {
                        Title = "Python para Dados",
                        Description = "Linguagem Python aplicada à análise e ciência de dados",
                        Category = "Análise de Dados",
                        EstimatedTime = "4h",
                        IsActive = true
                    }
                };

                context.Topics.AddRange(topics);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {topics.Count} tópicos criados com sucesso");

                // Add some sample documents and links for the first few topics
                await SeedTopicResources(context);
            }
            else
            {
                Console.WriteLine("ℹ️ Tópicos já existem no banco de dados");
            }
        }

        private static async Task SeedTopicResources(ApplicationDbContext context)
        {
            var sqlServerTopic = await context.Topics.FirstOrDefaultAsync(t => t.Title == "SQL Server");
            if (sqlServerTopic != null && !await context.TopicDocuments.AnyAsync(d => d.TopicId == sqlServerTopic.Id))
            {
                var documents = new List<TopicDocument>
                {
                    new TopicDocument
                    {
                        TopicId = sqlServerTopic.Id,
                        Title = "Manual SQL Server.pdf",
                        Type = "pdf",
                        Url = "#",
                        Size = "2.5 MB"
                    },
                    new TopicDocument
                    {
                        TopicId = sqlServerTopic.Id,
                        Title = "Guia de Consultas.pdf",
                        Type = "pdf",
                        Url = "#",
                        Size = "1.8 MB"
                    }
                };

                var links = new List<TopicLink>
                {
                    new TopicLink
                    {
                        TopicId = sqlServerTopic.Id,
                        Title = "Portal de Documentação Interna",
                        Url = "https://docs.vivo.com/sql"
                    },
                    new TopicLink
                    {
                        TopicId = sqlServerTopic.Id,
                        Title = "Tutorial SQL Server Microsoft",
                        Url = "https://docs.microsoft.com/sql"
                    }
                };

                var contacts = new List<TopicContact>
                {
                    new TopicContact
                    {
                        TopicId = sqlServerTopic.Id,
                        Name = "Ana Silva",
                        Role = "DBA Senior",
                        Email = "ana.silva@vivo.com",
                        Phone = "Ramal: 1234",
                        Department = "Infraestrutura"
                    }
                };

                context.TopicDocuments.AddRange(documents);
                context.TopicLinks.AddRange(links);
                context.TopicContacts.AddRange(contacts);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Recursos adicionais dos tópicos criados");
            }
        }
    }
}