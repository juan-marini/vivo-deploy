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
                    Console.WriteLine("📝 Criando perfis baseados em equipes...");

                    var perfis = new List<Perfil>
                    {
                        new Perfil
                        {
                            Nome = "Administrador",
                            Descricao = "Acesso total ao sistema"
                        },
                        new Perfil
                        {
                            Nome = "Gestão",
                            Descricao = "Gerencia equipes e visualiza dashboards"
                        },
                        new Perfil
                        {
                            Nome = "Desenvolvimento",
                            Descricao = "Equipe de desenvolvimento de software"
                        },
                        new Perfil
                        {
                            Nome = "Infraestrutura",
                            Descricao = "Equipe de infraestrutura, DevOps e Cloud"
                        },
                        new Perfil
                        {
                            Nome = "QA",
                            Descricao = "Equipe de qualidade e testes"
                        },
                        new Perfil
                        {
                            Nome = "Produto",
                            Descricao = "Equipe de produto e arquitetura"
                        },
                        new Perfil
                        {
                            Nome = "Dados",
                            Descricao = "Equipe de dados, BI e analytics"
                        },
                        new Perfil
                        {
                            Nome = "Design",
                            Descricao = "Equipe de UX/UI e design"
                        }
                    };

                    context.Perfis.AddRange(perfis);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Perfis de equipes criados com sucesso");
                }

                // Migrar usuários existentes para novos perfis baseados em equipes
                await MigrateUsersToTeamProfiles(context);

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

                // Criar usuários de teste - 35 colaboradores de diferentes setores de TI
                var usuariosTeste = new[]
                {
                    // Desenvolvimento
                    ("joao.silva@vivo.com.br", "João Silva", "Desenvolvimento", "Desenvolvedor Backend Senior"),
                    ("maria.oliveira@vivo.com.br", "Maria Oliveira", "Desenvolvimento", "Analista de Dados Senior"),
                    ("carlos.santos@vivo.com.br", "Carlos Santos", "Desenvolvimento", "Desenvolvedor Frontend Senior"),
                    ("ana.costa@vivo.com.br", "Ana Costa", "Desenvolvimento", "Desenvolvedora Full Stack"),
                    ("pedro.almeida@vivo.com.br", "Pedro Almeida", "Desenvolvimento", "Desenvolvedor Mobile"),
                    ("rafael.martins@vivo.com.br", "Rafael Martins", "Desenvolvimento", "Desenvolvedor Python"),
                    ("claudia.mendes@vivo.com.br", "Claudia Mendes", "Desenvolvimento", "Desenvolvedora React"),
                    ("felipe.castro@vivo.com.br", "Felipe Castro", "Desenvolvimento", "Desenvolvedor Node.js"),
                    ("beatriz.soares@vivo.com.br", "Beatriz Soares", "Desenvolvimento", "Desenvolvedora .NET"),

                    // Infraestrutura & DevOps
                    ("roberto.lima@vivo.com.br", "Roberto Lima", "DevOps", "Engenheiro DevOps"),
                    ("fernanda.reis@vivo.com.br", "Fernanda Reis", "Infraestrutura", "Administradora de Sistemas"),
                    ("lucas.pereira@vivo.com.br", "Lucas Pereira", "Cloud", "Especialista em Cloud AWS"),
                    ("andre.silva@vivo.com.br", "André Silva", "DevOps", "Especialista em Kubernetes"),
                    ("carolina.vieira@vivo.com.br", "Carolina Vieira", "Cloud", "Arquiteta de Cloud Azure"),
                    ("marcelo.costa@vivo.com.br", "Marcelo Costa", "Infraestrutura", "Especialista em Docker"),

                    // Segurança
                    ("juliana.campos@vivo.com.br", "Juliana Campos", "Segurança", "Analista de Segurança da Informação"),
                    ("ricardo.moura@vivo.com.br", "Ricardo Moura", "Segurança", "Especialista em Cybersecurity"),
                    ("isabela.melo@vivo.com.br", "Isabela Melo", "Segurança", "Analista de Vulnerabilidades"),
                    ("gustavo.rocha@vivo.com.br", "Gustavo Rocha", "Segurança", "Especialista em SOC"),

                    // Qualidade & Testes
                    ("camila.rocha@vivo.com.br", "Camila Rocha", "QA", "Analista de QA"),
                    ("thiago.melo@vivo.com.br", "Thiago Melo", "QA", "Engenheiro de Testes Automatizados"),
                    ("priscila.santos@vivo.com.br", "Priscila Santos", "QA", "Testadora Manual"),
                    ("leonardo.lima@vivo.com.br", "Leonardo Lima", "QA", "Especialista em Performance"),

                    // UX/UI & Design
                    ("patricia.souza@vivo.com.br", "Patricia Souza", "Design", "UX/UI Designer"),
                    ("gabriel.torres@vivo.com.br", "Gabriel Torres", "Design", "Designer de Produto"),
                    ("natalia.ferreira@vivo.com.br", "Natália Ferreira", "Design", "UX Researcher"),
                    ("diego.alves@vivo.com.br", "Diego Alves", "Design", "Designer Gráfico"),

                    // Dados & BI
                    ("amanda.ferreira@vivo.com.br", "Amanda Ferreira", "Dados", "Cientista de Dados"),
                    ("bruno.carvalho@vivo.com.br", "Bruno Carvalho", "Dados", "Engenheiro de Dados"),
                    ("vanessa.oliveira@vivo.com.br", "Vanessa Oliveira", "BI", "Analista de Business Intelligence"),
                    ("rodrigo.nascimento@vivo.com.br", "Rodrigo Nascimento", "Dados", "Especialista em Machine Learning"),

                    // Suporte & TI
                    ("larissa.gomes@vivo.com.br", "Larissa Gomes", "Suporte", "Analista de Suporte N2"),
                    ("eduardo.barbosa@vivo.com.br", "Eduardo Barbosa", "Banco de Dados", "Administrador de Banco de Dados"),
                    ("simone.araujo@vivo.com.br", "Simone Araújo", "Suporte", "Analista de Suporte N3"),
                    ("fabio.monteiro@vivo.com.br", "Fábio Monteiro", "TI", "Técnico de Redes"),

                    // Gestão & Arquitetura
                    ("mariana.nunes@vivo.com.br", "Mariana Nunes", "Arquitetura", "Arquiteta de Software"),
                    ("daniel.rodrigues@vivo.com.br", "Daniel Rodrigues", "Produto", "Product Owner"),
                    ("cristiane.lopes@vivo.com.br", "Cristiane Lopes", "Arquitetura", "Arquiteta de Soluções"),
                    ("henrique.silva@vivo.com.br", "Henrique Silva", "Gestão", "Scrum Master")
                };

                // Criar usuário gestor
                var perfilGestor = await context.Perfis.FirstAsync(p => p.Nome == "Gestão");
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

                // Buscar perfis por equipe
                var perfilDesenvolvimento = await context.Perfis.FirstAsync(p => p.Nome == "Desenvolvimento");
                var perfilInfraestrutura = await context.Perfis.FirstAsync(p => p.Nome == "Infraestrutura");
                var perfilQA = await context.Perfis.FirstAsync(p => p.Nome == "QA");
                var perfilProduto = await context.Perfis.FirstAsync(p => p.Nome == "Produto");
                var perfilDados = await context.Perfis.FirstAsync(p => p.Nome == "Dados");
                var perfilDesign = await context.Perfis.FirstAsync(p => p.Nome == "Design");

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
                        // Determinar perfil baseado no departamento
                        int perfilId = depto switch
                        {
                            "Desenvolvimento" => perfilDesenvolvimento.Id,
                            "DevOps" or "Infraestrutura" or "Cloud" => perfilInfraestrutura.Id,
                            "QA" => perfilQA.Id,
                            "Arquitetura" or "Produto" or "Gestão" => perfilProduto.Id,
                            "Dados" or "BI" => perfilDados.Id,
                            "Design" => perfilDesign.Id,
                            "Segurança" or "Suporte" or "Banco de Dados" or "TI" => perfilInfraestrutura.Id, // Agrupa com Infra
                            _ => perfilDesenvolvimento.Id // Default
                        };

                        var usuario = new Usuario
                        {
                            Email = email,
                            Senha = BCrypt.Net.BCrypt.HashPassword("Senha@123"),
                            NomeCompleto = nome,
                            PerfilId = perfilId,
                            Ativo = true,
                            PrimeiroAcesso = true,
                            Departamento = depto,
                            Cargo = cargo,
                            GestorId = adminUserRef.Id,
                            DataAdmissao = DateTime.Now.AddDays(Random.Shared.Next(-90, -10)) // Datas de admissão variadas
                        };

                        context.Usuarios.Add(usuario);
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Usuários de teste criados");

                // Seed Topics
                await SeedTopics(context);

                // Seed Member Progress - COMENTADO PARA EVITAR DADOS MOCADOS
                // await SeedMemberProgress(context);

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

        // FUNÇÃO COMENTADA PARA EVITAR CRIAÇÃO DE DADOS MOCADOS DE PROGRESSO
        /*
        private static async Task SeedMemberProgress(ApplicationDbContext context)
        {
            if (!await context.MemberProgresses.AnyAsync())
            {
                Console.WriteLine("📊 Criando progresso inicial dos colaboradores...");

                var topics = await context.Topics.ToListAsync();
                if (!topics.Any())
                {
                    Console.WriteLine("⚠️ Nenhum tópico encontrado para criar progresso");
                    return;
                }

                var progressData = new List<MemberProgress>();

                // João Silva - 60% (3 de 5 completos)
                progressData.AddRange(new[]
                {
                    new MemberProgress { UserId = "joao.silva@vivo.com.br", TopicId = topics[0].Id, IsCompleted = true, CompletedDate = DateTime.Now.AddDays(-5), StartedDate = DateTime.Now.AddDays(-7) },
                    new MemberProgress { UserId = "joao.silva@vivo.com.br", TopicId = topics[1].Id, IsCompleted = true, CompletedDate = DateTime.Now.AddDays(-3), StartedDate = DateTime.Now.AddDays(-5) },
                    new MemberProgress { UserId = "joao.silva@vivo.com.br", TopicId = topics[2].Id, IsCompleted = true, CompletedDate = DateTime.Now.AddDays(-1), StartedDate = DateTime.Now.AddDays(-3) },
                    new MemberProgress { UserId = "joao.silva@vivo.com.br", TopicId = topics[3].Id, IsCompleted = false, StartedDate = DateTime.Now.AddDays(-1) },
                    new MemberProgress { UserId = "joao.silva@vivo.com.br", TopicId = topics[4].Id, IsCompleted = false, StartedDate = DateTime.Now.AddDays(-1) }
                });

                // Maria Oliveira - 50% (1 de 2)
                if (topics.Count > 7)
                {
                    progressData.AddRange(new[]
                    {
                        new MemberProgress { UserId = "maria.oliveira@vivo.com.br", TopicId = topics[7].Id, IsCompleted = true, CompletedDate = DateTime.Now.AddDays(-2), StartedDate = DateTime.Now.AddDays(-4) },
                        new MemberProgress { UserId = "maria.oliveira@vivo.com.br", TopicId = topics[8].Id, IsCompleted = false, StartedDate = DateTime.Now.AddDays(-2) }
                    });
                }

                // Ana Costa - 67% (2 de 3)
                progressData.AddRange(new[]
                {
                    new MemberProgress { UserId = "ana.costa@vivo.com.br", TopicId = topics[0].Id, IsCompleted = true, CompletedDate = DateTime.Now.AddDays(-4), StartedDate = DateTime.Now.AddDays(-6) },
                    new MemberProgress { UserId = "ana.costa@vivo.com.br", TopicId = topics[1].Id, IsCompleted = true, CompletedDate = DateTime.Now.AddDays(-2), StartedDate = DateTime.Now.AddDays(-4) },
                    new MemberProgress { UserId = "ana.costa@vivo.com.br", TopicId = topics[2].Id, IsCompleted = false, StartedDate = DateTime.Now.AddDays(-2) }
                });

                // Carlos Santos - 50% (1 de 2)
                progressData.AddRange(new[]
                {
                    new MemberProgress { UserId = "carlos.santos@vivo.com.br", TopicId = topics[0].Id, IsCompleted = true, CompletedDate = DateTime.Now.AddDays(-6), StartedDate = DateTime.Now.AddDays(-8) },
                    new MemberProgress { UserId = "carlos.santos@vivo.com.br", TopicId = topics[3].Id, IsCompleted = false, StartedDate = DateTime.Now.AddDays(-1) }
                });

                context.MemberProgresses.AddRange(progressData);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {progressData.Count} registros de progresso criados");

                // Mostrar estatísticas
                var stats = await context.MemberProgresses
                    .GroupBy(mp => mp.UserId)
                    .Select(g => new {
                        UserId = g.Key,
                        Total = g.Count(),
                        Completed = g.Count(mp => mp.IsCompleted),
                        Percentage = (double)g.Count(mp => mp.IsCompleted) / g.Count() * 100
                    })
                    .ToListAsync();

                Console.WriteLine("\n📈 Progresso dos colaboradores:");
                foreach (var stat in stats)
                {
                    Console.WriteLine($"   {stat.UserId}: {stat.Percentage:F0}% ({stat.Completed}/{stat.Total})");
                }
                Console.WriteLine("");
            }
            else
            {
                Console.WriteLine("ℹ️ Dados de progresso já existem no banco");
            }
        }
        */

        private static async Task MigrateUsersToTeamProfiles(ApplicationDbContext context)
        {
            try
            {
                Console.WriteLine("🔄 Migrando usuários para perfis baseados em equipes...");

                // Buscar todos os perfis de equipe
                var perfilDesenvolvimento = await context.Perfis.FirstAsync(p => p.Nome == "Desenvolvimento");
                var perfilInfraestrutura = await context.Perfis.FirstAsync(p => p.Nome == "Infraestrutura");
                var perfilQA = await context.Perfis.FirstAsync(p => p.Nome == "QA");
                var perfilProduto = await context.Perfis.FirstAsync(p => p.Nome == "Produto");
                var perfilDados = await context.Perfis.FirstAsync(p => p.Nome == "Dados");
                var perfilDesign = await context.Perfis.FirstAsync(p => p.Nome == "Design");
                var perfilGestao = await context.Perfis.FirstAsync(p => p.Nome == "Gestão");
                var perfilAdmin = await context.Perfis.FirstAsync(p => p.Nome == "Administrador");

                // Buscar usuários que precisam ser migrados
                var usuarios = await context.Usuarios.Where(u => u.Ativo).ToListAsync();
                int migrated = 0;

                foreach (var usuario in usuarios)
                {
                    int novoPerfilId = 0;

                    // Lógica de mapeamento baseada no departamento/cargo
                    if (usuario.Email.Contains("admin"))
                    {
                        novoPerfilId = perfilAdmin.Id;
                    }
                    else if (usuario.Departamento != null)
                    {
                        novoPerfilId = usuario.Departamento switch
                        {
                            "Desenvolvimento" => perfilDesenvolvimento.Id,
                            "DevOps" or "Infraestrutura" or "Cloud" => perfilInfraestrutura.Id,
                            "QA" => perfilQA.Id,
                            "Arquitetura" or "Produto" => perfilProduto.Id,
                            "Dados" or "BI" => perfilDados.Id,
                            "Design" => perfilDesign.Id,
                            "Gestão" => perfilGestao.Id,
                            "Segurança" or "Suporte" or "Banco de Dados" or "TI" => perfilInfraestrutura.Id,
                            _ => perfilDesenvolvimento.Id // Default para desenvolvimento
                        };
                    }
                    else
                    {
                        // Fallback para desenvolvimento se não tiver departamento
                        novoPerfilId = perfilDesenvolvimento.Id;
                    }

                    // Atualizar o perfil do usuário se necessário
                    if (usuario.PerfilId != novoPerfilId)
                    {
                        usuario.PerfilId = novoPerfilId;
                        migrated++;
                    }
                }

                if (migrated > 0)
                {
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ {migrated} usuários migrados para perfis baseados em equipes");
                }
                else
                {
                    Console.WriteLine("ℹ️ Todos os usuários já estão com perfis corretos");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao migrar usuários: {ex.Message}");
            }
        }
    }
}