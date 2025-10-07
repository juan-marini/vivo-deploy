using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using backend.Services.Interfaces;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _progressService;
        private readonly ApplicationDbContext _context;

        public ProgressController(IProgressService progressService, ApplicationDbContext context)
        {
            _progressService = progressService;
            _context = context;
        }

        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<MemberDetailedProgressDto>> GetMemberDetailedProgress(string memberId)
        {
            try
            {
                var result = await _progressService.GetMemberDetailedProgressAsync(memberId);
                if (result == null)
                {
                    return NotFound($"Member with ID {memberId} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving member progress: {ex.Message}");
            }
        }

        [HttpGet("members")]
        public async Task<ActionResult<List<TeamMemberDto>>> GetAllMembers()
        {
            try
            {
                var members = await _progressService.GetAllMembersAsync();
                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving members: {ex.Message}");
            }
        }

        [HttpPost("member/{memberId}/topic/{topicId}/complete")]
        public async Task<ActionResult> MarkTopicAsCompleted(string memberId, int topicId)
        {
            try
            {
                await _progressService.MarkTopicAsCompletedAsync(memberId, topicId);
                return Ok(new { message = "Topic marked as completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error marking topic as completed: {ex.Message}");
            }
        }

        [HttpPost("member/{memberId}/activity")]
        public async Task<ActionResult> AddActivity(string memberId, [FromBody] ActivityItemDto activity)
        {
            try
            {
                await _progressService.AddActivityAsync(memberId, activity);
                return Ok(new { message = "Activity added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error adding activity: {ex.Message}");
            }
        }

        [HttpGet("member/{memberId}/activity")]
        public async Task<ActionResult<List<ActivityItemDto>>> GetMemberActivity(string memberId)
        {
            try
            {
                var activities = await _progressService.GetMemberActivityAsync(memberId);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving member activity: {ex.Message}");
            }
        }

        [HttpGet("topics")]
        public async Task<ActionResult<List<TopicDto>>> GetAllTopics()
        {
            try
            {
                Console.WriteLine("🔍 Fetching topics from database...");
                var topics = await _progressService.GetAllTopicsAsync();
                Console.WriteLine($"✅ Retrieved {topics.Count} topics successfully");
                return Ok(topics);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetAllTopics: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                Console.WriteLine($"❌ Inner exception: {ex.InnerException?.Message}");

                return BadRequest(new {
                    error = "Error retrieving topics",
                    message = ex.Message,
                    innerError = ex.InnerException?.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("member/{memberId}/topics")]
        public async Task<ActionResult<List<MemberTopicProgressDto>>> GetMemberTopicsProgress(string memberId)
        {
            try
            {
                var topicsProgress = await _progressService.GetMemberTopicsProgressAsync(memberId);
                return Ok(topicsProgress);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving member topics progress: {ex.Message}");
            }
        }

        [HttpPost("fix-database")]
        public async Task<ActionResult> FixDatabase()
        {
            try
            {
                var results = new List<string>();

                // 1. Create missing tables if they don't exist
                try
                {
                    await _context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS topicdocuments (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            TopicId INT NOT NULL,
                            Title VARCHAR(200) NOT NULL,
                            Type VARCHAR(20) NOT NULL,
                            Url VARCHAR(500) NOT NULL,
                            Size VARCHAR(20) NULL,
                            FOREIGN KEY (TopicId) REFERENCES topics(Id)
                        )");
                    results.Add("✅ Table topicdocuments created or already exists");
                }
                catch (Exception ex)
                {
                    results.Add($"ℹ️ Table topicdocuments: {ex.Message}");
                }

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS topiclinks (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            TopicId INT NOT NULL,
                            Title VARCHAR(200) NOT NULL,
                            Url VARCHAR(500) NOT NULL,
                            FOREIGN KEY (TopicId) REFERENCES topics(Id)
                        )");
                    results.Add("✅ Table topiclinks created or already exists");
                }
                catch (Exception ex)
                {
                    results.Add($"ℹ️ Table topiclinks: {ex.Message}");
                }

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS topiccontacts (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            TopicId INT NOT NULL,
                            Name VARCHAR(100) NOT NULL,
                            Role VARCHAR(100) NOT NULL,
                            Email VARCHAR(100) NOT NULL,
                            Phone VARCHAR(50) NOT NULL,
                            Department VARCHAR(100) NOT NULL,
                            FOREIGN KEY (TopicId) REFERENCES topics(Id)
                        )");
                    results.Add("✅ Table topiccontacts created or already exists");
                }
                catch (Exception ex)
                {
                    results.Add($"ℹ️ Table topiccontacts: {ex.Message}");
                }

                // 2. Clear member progress data
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM member_progresses");
                results.Add("✅ Progress data cleared");

                // 3. Add EstimatedTime column if it doesn't exist (handle duplicate error)
                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE topics ADD COLUMN EstimatedTime VARCHAR(20) DEFAULT '2h'");
                    results.Add("✅ EstimatedTime column added");
                }
                catch (Exception ex) when (ex.Message.Contains("Duplicate column"))
                {
                    results.Add("ℹ️ EstimatedTime column already exists");
                }

                // 3. Add IsActive column if it doesn't exist (handle duplicate error)
                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE topics ADD COLUMN IsActive BOOLEAN DEFAULT TRUE");
                    results.Add("✅ IsActive column added");
                }
                catch (Exception ex) when (ex.Message.Contains("Duplicate column"))
                {
                    results.Add("ℹ️ IsActive column already exists");
                }

                // 4. Add missing columns to member_progresses table
                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE member_progresses ADD COLUMN StartedDate DATETIME");
                    results.Add("✅ StartedDate column added to member_progresses");
                }
                catch (Exception ex) when (ex.Message.Contains("Duplicate column"))
                {
                    results.Add("ℹ️ StartedDate column already exists in member_progresses");
                }

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE member_progresses ADD COLUMN CompletedDate DATETIME");
                    results.Add("✅ CompletedDate column added to member_progresses");
                }
                catch (Exception ex) when (ex.Message.Contains("Duplicate column"))
                {
                    results.Add("ℹ️ CompletedDate column already exists in member_progresses");
                }

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE member_progresses ADD COLUMN TimeSpent VARCHAR(20)");
                    results.Add("✅ TimeSpent column added to member_progresses");
                }
                catch (Exception ex) when (ex.Message.Contains("Duplicate column"))
                {
                    results.Add("ℹ️ TimeSpent column already exists in member_progresses");
                }

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE member_progresses ADD COLUMN IsCompleted BOOLEAN DEFAULT FALSE");
                    results.Add("✅ IsCompleted column added to member_progresses");
                }
                catch (Exception ex) when (ex.Message.Contains("Duplicate column"))
                {
                    results.Add("ℹ️ IsCompleted column already exists in member_progresses");
                }

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE member_progresses ADD COLUMN Notes TEXT");
                    results.Add("✅ Notes column added to member_progresses");
                }
                catch (Exception ex) when (ex.Message.Contains("Duplicate column"))
                {
                    results.Add("ℹ️ Notes column already exists in member_progresses");
                }

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE member_progresses ADD COLUMN TopicId INT");
                    results.Add("✅ TopicId column added to member_progresses");
                }
                catch (Exception ex) when (ex.Message.Contains("Duplicate column"))
                {
                    results.Add("ℹ️ TopicId column already exists in member_progresses");
                }

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE member_progresses ADD COLUMN UserId VARCHAR(255)");
                    results.Add("✅ UserId column added to member_progresses");
                }
                catch (Exception ex) when (ex.Message.Contains("Duplicate column"))
                {
                    results.Add("ℹ️ UserId column already exists in member_progresses");
                }

                // 5. Update all topics with proper values
                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE topics SET
                        EstimatedTime = CASE
                            WHEN Title LIKE '%SQL Server%' THEN '2h'
                            WHEN Title LIKE '%Oracle%' THEN '1.5h'
                            WHEN Title LIKE '%MongoDB%' THEN '3h'
                            WHEN Title LIKE '%Angular%' THEN '4h'
                            WHEN Title LIKE '%React%' THEN '3.5h'
                            WHEN Title LIKE '%ASP.NET Core%' THEN '5h'
                            WHEN Title LIKE '%Docker%' THEN '2.5h'
                            WHEN Title LIKE '%Kubernetes%' THEN '6h'
                            WHEN Title LIKE '%Power BI%' THEN '3h'
                            WHEN Title LIKE '%Python%' THEN '4h'
                            ELSE '2h'
                        END,
                        IsActive = TRUE
                    WHERE EstimatedTime IS NULL OR EstimatedTime = '' OR IsActive IS NULL");
                results.Add("✅ Topics updated with proper values");

                // 6. Get verification data
                var topicCount = await _context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM topics");
                var progressCount = await _context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM member_progresses");

                results.Add($"📊 Total topics: {topicCount}");
                results.Add($"📊 Progress records: {progressCount} (should be 0)");
                results.Add("🎉 Database schema fixed successfully! All colaboradores reset to 0%!");

                // NOVA FUNCIONALIDADE: Limpeza de tabelas desnecessárias
                results.Add("🧹 Iniciando limpeza de tabelas desnecessárias...");

                var tablesToRemove = new[] {
                    "contatos_referencia",
                    "links_uteis",
                    "logs_auditoria",
                    "notificacoes",
                    "configuracoes",
                    "categorias_topicos",
                    "progresso_usuarios"
                };

                foreach (var table in tablesToRemove)
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS {table}");
                        results.Add($"🗑️ Tabela '{table}' removida");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"ℹ️ Tabela '{table}' não existia ou erro: {ex.Message}");
                    }
                }

                results.Add("✅ Limpeza de tabelas concluída!");

                return Ok(new {
                    success = true,
                    message = "Database fixed and cleaned successfully",
                    details = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fixing database: {ex.Message}");
            }
        }

        [HttpGet("analyze-tables")]
        public async Task<ActionResult> AnalyzeTables()
        {
            try
            {
                var results = new List<object>();

                // Verificar contagem de registros em cada tabela
                var tableQueries = new Dictionary<string, string>
                {
                    {"member_progresses", "SELECT COUNT(*) FROM member_progresses"},
                    {"progresso_usuarios", "SELECT COUNT(*) FROM progresso_usuarios"},
                    {"topics", "SELECT COUNT(*) FROM topics"},
                    {"topicos", "SELECT COUNT(*) FROM topicos"},
                    {"categorias_topicos", "SELECT COUNT(*) FROM categorias_topicos"},
                    {"contatos_referencia", "SELECT COUNT(*) FROM contatos_referencia"},
                    {"links_uteis", "SELECT COUNT(*) FROM links_uteis"},
                    {"logs_auditoria", "SELECT COUNT(*) FROM logs_auditoria"},
                    {"notificacoes", "SELECT COUNT(*) FROM notificacoes"},
                    {"configuracoes", "SELECT COUNT(*) FROM configuracoes"}
                };

                foreach (var table in tableQueries)
                {
                    try
                    {
                        var connection = _context.Database.GetDbConnection();
                        await connection.OpenAsync();
                        var command = connection.CreateCommand();
                        command.CommandText = table.Value;
                        var count = await command.ExecuteScalarAsync();
                        await connection.CloseAsync();

                        results.Add(new {
                            table = table.Key,
                            count = count?.ToString() ?? "0",
                            status = int.Parse(count?.ToString() ?? "0") > 0 ? "COM DADOS" : "VAZIA"
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new {
                            table = table.Key,
                            count = "ERROR",
                            status = $"ERRO: {ex.Message}",
                            error = ex.Message
                        });
                    }
                }

                return Ok(new {
                    success = true,
                    message = "Table analysis completed",
                    tables = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error analyzing tables: {ex.Message}");
            }
        }

        [HttpPost("cleanup-unused-tables")]
        public async Task<ActionResult> CleanupUnusedTables()
        {
            try
            {
                var results = new List<string>();

                // Primeiro, fazer backup das tabelas importantes
                results.Add("🔍 Analisando tabelas para limpeza...");

                // Identificar tabelas vazias/desnecessárias e excluir
                var tablesToCheck = new[] {
                    "contatos_referencia",
                    "links_uteis",
                    "logs_auditoria",
                    "notificacoes",
                    "configuracoes",
                    "categorias_topicos"
                };

                foreach (var table in tablesToCheck)
                {
                    try
                    {
                        // Verificar se tem dados
                        var connection = _context.Database.GetDbConnection();
                        await connection.OpenAsync();
                        var countCommand = connection.CreateCommand();
                        countCommand.CommandText = $"SELECT COUNT(*) FROM {table}";
                        var count = await countCommand.ExecuteScalarAsync();
                        var recordCount = int.Parse(count?.ToString() ?? "0");

                        if (recordCount == 0)
                        {
                            // Tabela vazia - pode excluir
                            var dropCommand = connection.CreateCommand();
                            dropCommand.CommandText = $"DROP TABLE IF EXISTS {table}";
                            await dropCommand.ExecuteNonQueryAsync();
                            results.Add($"🗑️ Tabela '{table}' excluída (estava vazia)");
                        }
                        else
                        {
                            results.Add($"✅ Tabela '{table}' mantida ({recordCount} registros)");
                        }

                        await connection.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        results.Add($"❌ Erro ao verificar '{table}': {ex.Message}");
                    }
                }

                // Verificar se existe duplicação entre topics e topicos
                try
                {
                    var connection = _context.Database.GetDbConnection();
                    await connection.OpenAsync();

                    var topicsCommand = connection.CreateCommand();
                    topicsCommand.CommandText = "SELECT COUNT(*) FROM topics";
                    var topicsCount = int.Parse((await topicsCommand.ExecuteScalarAsync())?.ToString() ?? "0");

                    var topicosCommand = connection.CreateCommand();
                    topicosCommand.CommandText = "SELECT COUNT(*) FROM topicos";
                    var topicosCount = int.Parse((await topicosCommand.ExecuteScalarAsync())?.ToString() ?? "0");

                    await connection.CloseAsync();

                    if (topicsCount > 0 && topicosCount == 0)
                    {
                        await _context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS topicos");
                        results.Add("🗑️ Tabela 'topicos' excluída (duplicada e vazia)");
                    }
                    else if (topicosCount > 0 && topicsCount == 0)
                    {
                        await _context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS topics");
                        results.Add("🗑️ Tabela 'topics' excluída (duplicada e vazia)");
                    }
                    else
                    {
                        results.Add($"ℹ️ Mantendo ambas: topics({topicsCount}) e topicos({topicosCount})");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"❌ Erro ao verificar topics/topicos: {ex.Message}");
                }

                results.Add("🎉 Limpeza de tabelas concluída!");

                return Ok(new {
                    success = true,
                    message = "Table cleanup completed",
                    details = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error during cleanup: {ex.Message}");
            }
        }

        [HttpPost("remove-unused-tables")]
        public async Task<ActionResult> RemoveUnusedTables()
        {
            try
            {
                var results = new List<string>();
                results.Add("🧹 Iniciando remoção de tabelas desnecessárias...");

                // Lista de tabelas que provavelmente estão vazias/desnecessárias
                var tablesToRemove = new[] {
                    "contatos_referencia",
                    "links_uteis",
                    "logs_auditoria",
                    "notificacoes",
                    "configuracoes",
                    "categorias_topicos"
                };

                foreach (var table in tablesToRemove)
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS {table}");
                        results.Add($"🗑️ Tabela '{table}' removida");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"❌ Erro ao remover '{table}': {ex.Message}");
                    }
                }

                // Verificar e remover duplicata de topics/topicos
                try
                {
                    // Primeiro verificar qual tem dados
                    var connection = _context.Database.GetDbConnection();
                    await connection.OpenAsync();

                    var topicsCommand = connection.CreateCommand();
                    topicsCommand.CommandText = "SELECT COUNT(*) FROM topics WHERE 1=1";
                    var topicsExists = true;
                    var topicsCount = 0;
                    try
                    {
                        topicsCount = int.Parse((await topicsCommand.ExecuteScalarAsync())?.ToString() ?? "0");
                    }
                    catch
                    {
                        topicsExists = false;
                    }

                    var topicosCommand = connection.CreateCommand();
                    topicosCommand.CommandText = "SELECT COUNT(*) FROM topicos WHERE 1=1";
                    var topicosExists = true;
                    var topicosCount = 0;
                    try
                    {
                        topicosCount = int.Parse((await topicosCommand.ExecuteScalarAsync())?.ToString() ?? "0");
                    }
                    catch
                    {
                        topicosExists = false;
                    }

                    await connection.CloseAsync();

                    // Remover a tabela vazia se ambas existem
                    if (topicsExists && topicosExists)
                    {
                        if (topicsCount > 0 && topicosCount == 0)
                        {
                            await _context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS topicos");
                            results.Add("🗑️ Tabela 'topicos' removida (duplicada e vazia)");
                        }
                        else if (topicosCount > 0 && topicsCount == 0)
                        {
                            await _context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS topics");
                            results.Add("🗑️ Tabela 'topics' removida (duplicada e vazia)");
                        }
                        else
                        {
                            results.Add($"ℹ️ Mantendo ambas tabelas: topics({topicsCount}) e topicos({topicosCount})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"❌ Erro ao verificar topics/topicos: {ex.Message}");
                }

                // Verificar progresso_usuarios vs member_progresses
                try
                {
                    var connection = _context.Database.GetDbConnection();
                    await connection.OpenAsync();

                    var memberProgressCommand = connection.CreateCommand();
                    memberProgressCommand.CommandText = "SELECT COUNT(*) FROM member_progresses";
                    var memberProgressCount = int.Parse((await memberProgressCommand.ExecuteScalarAsync())?.ToString() ?? "0");

                    var progressoUsuariosCommand = connection.CreateCommand();
                    progressoUsuariosCommand.CommandText = "SELECT COUNT(*) FROM progresso_usuarios";
                    var progressoUsuariosCount = 0;
                    try
                    {
                        progressoUsuariosCount = int.Parse((await progressoUsuariosCommand.ExecuteScalarAsync())?.ToString() ?? "0");
                    }
                    catch
                    {
                        // Tabela não existe
                        results.Add("ℹ️ Tabela 'progresso_usuarios' não existe");
                    }

                    await connection.CloseAsync();

                    // Se member_progresses está vazia e progresso_usuarios tem dados, remover member_progresses
                    if (memberProgressCount == 0 && progressoUsuariosCount > 0)
                    {
                        results.Add($"ℹ️ member_progresses está vazia ({memberProgressCount}) e progresso_usuarios tem dados ({progressoUsuariosCount})");
                        results.Add("ℹ️ Mantendo ambas por segurança - o sistema usa member_progresses");
                    }
                    else if (progressoUsuariosCount == 0 && memberProgressCount >= 0)
                    {
                        // Remover progresso_usuarios se está vazia
                        try
                        {
                            await _context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS progresso_usuarios");
                            results.Add("🗑️ Tabela 'progresso_usuarios' removida (vazia/não utilizada)");
                        }
                        catch (Exception ex)
                        {
                            results.Add($"❌ Erro ao remover progresso_usuarios: {ex.Message}");
                        }
                    }
                    else
                    {
                        results.Add($"ℹ️ Mantendo ambas: member_progresses({memberProgressCount}) e progresso_usuarios({progressoUsuariosCount})");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"❌ Erro ao verificar tabelas de progresso: {ex.Message}");
                }

                results.Add("✅ Limpeza de tabelas concluída!");

                return Ok(new {
                    success = true,
                    message = "Unused tables removed successfully",
                    details = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error removing unused tables: {ex.Message}");
            }
        }

        [HttpPost("populate-topic-content")]
        public async Task<ActionResult> PopulateTopicContent()
        {
            try
            {
                var results = new List<string>();
                results.Add("🎯 Populando conteúdo dos tópicos...");

                // Documentos
                var documents = new[]
                {
                    new { TopicId = 3, Title = "Manual do Colaborador Vivo", Type = "pdf", Url = "/docs/manual-colaborador-vivo.pdf", Size = "2.5MB" },
                    new { TopicId = 3, Title = "Código de Ética e Conduta", Type = "pdf", Url = "/docs/codigo-etica-vivo.pdf", Size = "1.2MB" },
                    new { TopicId = 3, Title = "Organograma da Empresa", Type = "pdf", Url = "/docs/organograma-vivo.pdf", Size = "800KB" },
                    new { TopicId = 4, Title = "Política de Segurança da Informação", Type = "pdf", Url = "/docs/politica-seguranca-vivo.pdf", Size = "1.8MB" },
                    new { TopicId = 4, Title = "Guia de Boas Práticas de Segurança", Type = "pdf", Url = "/docs/guia-seguranca-vivo.pdf", Size = "3.2MB" },
                    new { TopicId = 4, Title = "Procedimentos de Backup", Type = "doc", Url = "/docs/procedimentos-backup.docx", Size = "500KB" },
                    new { TopicId = 5, Title = "Guia de Instalação Visual Studio", Type = "pdf", Url = "/docs/instalacao-visual-studio.pdf", Size = "2.1MB" },
                    new { TopicId = 5, Title = "Configuração do Ambiente de Desenvolvimento", Type = "pdf", Url = "/docs/config-ambiente-dev.pdf", Size = "1.5MB" },
                    new { TopicId = 5, Title = "Manual do Jira", Type = "doc", Url = "/docs/manual-jira.docx", Size = "900KB" },
                    new { TopicId = 6, Title = "Metodologia Scrum na Vivo", Type = "pdf", Url = "/docs/scrum-vivo.pdf", Size = "2.8MB" },
                    new { TopicId = 6, Title = "Template de Definition of Done", Type = "doc", Url = "/docs/template-dod.docx", Size = "300KB" },
                    new { TopicId = 6, Title = "Fluxo de Code Review", Type = "pdf", Url = "/docs/fluxo-code-review.pdf", Size = "1.1MB" },
                    new { TopicId = 7, Title = "Guia de Boas Práticas C#", Type = "pdf", Url = "/docs/boas-praticas-csharp.pdf", Size = "2.5MB" },
                    new { TopicId = 7, Title = "Convenções de Nomenclatura", Type = "doc", Url = "/docs/convencoes-nomenclatura.docx", Size = "600KB" },
                    new { TopicId = 7, Title = "Clean Code Guidelines", Type = "pdf", Url = "/docs/clean-code-guidelines.pdf", Size = "1.9MB" },
                    new { TopicId = 8, Title = "Git Flow na Vivo", Type = "pdf", Url = "/docs/git-flow-vivo.pdf", Size = "1.4MB" },
                    new { TopicId = 8, Title = "Comandos Git Essenciais", Type = "doc", Url = "/docs/comandos-git.docx", Size = "400KB" },
                    new { TopicId = 9, Title = "Guia de Testes Unitários", Type = "pdf", Url = "/docs/guia-testes-unitarios.pdf", Size = "2.2MB" },
                    new { TopicId = 9, Title = "Framework de Testes Vivo", Type = "pdf", Url = "/docs/framework-testes-vivo.pdf", Size = "1.7MB" },
                    new { TopicId = 10, Title = "Pipeline CI/CD Vivo", Type = "pdf", Url = "/docs/pipeline-cicd-vivo.pdf", Size = "3.1MB" },
                    new { TopicId = 10, Title = "Docker na Vivo", Type = "pdf", Url = "/docs/docker-vivo.pdf", Size = "2.4MB" },
                    new { TopicId = 11, Title = "Guia de Monitoramento", Type = "pdf", Url = "/docs/guia-monitoramento.pdf", Size = "1.8MB" },
                    new { TopicId = 12, Title = "Template de Documentação Técnica", Type = "doc", Url = "/docs/template-doc-tecnica.docx", Size = "700KB" }
                };

                // Links
                var links = new[]
                {
                    new { TopicId = 3, Title = "Portal do Colaborador", Url = "https://portal.vivo.com.br" },
                    new { TopicId = 3, Title = "Intranet Vivo", Url = "https://intranet.vivo.com.br" },
                    new { TopicId = 3, Title = "Benefícios Vivo", Url = "https://beneficios.vivo.com.br" },
                    new { TopicId = 4, Title = "Central de Segurança", Url = "https://seguranca.vivo.com.br" },
                    new { TopicId = 4, Title = "Treinamento de Segurança", Url = "https://treinamento-seguranca.vivo.com.br" },
                    new { TopicId = 5, Title = "Visual Studio Download", Url = "https://visualstudio.microsoft.com" },
                    new { TopicId = 5, Title = "Jira Vivo", Url = "https://jira.vivo.com.br" },
                    new { TopicId = 5, Title = "Confluence Vivo", Url = "https://confluence.vivo.com.br" },
                    new { TopicId = 6, Title = "Scrum Guide", Url = "https://scrumguides.org" },
                    new { TopicId = 6, Title = "Agile Manifesto", Url = "https://agilemanifesto.org" },
                    new { TopicId = 7, Title = "Microsoft C# Guidelines", Url = "https://docs.microsoft.com/en-us/dotnet/csharp" },
                    new { TopicId = 7, Title = "Clean Code Book", Url = "https://www.oreilly.com/library/view/clean-code-a/9780136083238" },
                    new { TopicId = 8, Title = "Git Documentation", Url = "https://git-scm.com/doc" },
                    new { TopicId = 8, Title = "GitHub Vivo", Url = "https://github.com/vivo" },
                    new { TopicId = 9, Title = "XUnit Documentation", Url = "https://xunit.net" },
                    new { TopicId = 9, Title = "NUnit Framework", Url = "https://nunit.org" },
                    new { TopicId = 10, Title = "Azure DevOps Vivo", Url = "https://dev.azure.com/vivo" },
                    new { TopicId = 10, Title = "Docker Hub", Url = "https://hub.docker.com" },
                    new { TopicId = 11, Title = "Grafana Vivo", Url = "https://grafana.vivo.com.br" },
                    new { TopicId = 11, Title = "Prometheus Documentation", Url = "https://prometheus.io/docs" },
                    new { TopicId = 12, Title = "Confluence Templates", Url = "https://www.atlassian.com/software/confluence/templates" }
                };

                // Contatos
                var contacts = new[]
                {
                    new { TopicId = 3, Name = "Ana Gestora", Role = "Gerente de RH", Email = "ana.gestora@vivo.com.br", Phone = "(11) 9999-0001", Department = "Recursos Humanos" },
                    new { TopicId = 3, Name = "Carlos Onboarding", Role = "Especialista em Integração", Email = "carlos.onboarding@vivo.com.br", Phone = "(11) 9999-0002", Department = "Recursos Humanos" },
                    new { TopicId = 4, Name = "Maria Segurança", Role = "Analista de Segurança", Email = "maria.seguranca@vivo.com.br", Phone = "(11) 9999-0003", Department = "Segurança da Informação" },
                    new { TopicId = 4, Name = "Roberto Cyber", Role = "Especialista em Cybersecurity", Email = "roberto.cyber@vivo.com.br", Phone = "(11) 9999-0004", Department = "Segurança da Informação" },
                    new { TopicId = 5, Name = "Paulo Ferramentas", Role = "Administrador de Sistemas", Email = "paulo.ferramentas@vivo.com.br", Phone = "(11) 9999-0005", Department = "TI" },
                    new { TopicId = 5, Name = "Julia Suporte", Role = "Analista de Suporte", Email = "julia.suporte@vivo.com.br", Phone = "(11) 9999-0006", Department = "TI" },
                    new { TopicId = 6, Name = "Fernando Agile", Role = "Scrum Master", Email = "fernando.agile@vivo.com.br", Phone = "(11) 9999-0007", Department = "Desenvolvimento" },
                    new { TopicId = 6, Name = "Patrícia Product", Role = "Product Owner", Email = "patricia.product@vivo.com.br", Phone = "(11) 9999-0008", Department = "Produto" },
                    new { TopicId = 7, Name = "Ricardo Senior", Role = "Desenvolvedor Sênior", Email = "ricardo.senior@vivo.com.br", Phone = "(11) 9999-0009", Department = "Desenvolvimento" },
                    new { TopicId = 7, Name = "Luciana Arch", Role = "Arquiteta de Software", Email = "luciana.arch@vivo.com.br", Phone = "(11) 9999-0010", Department = "Arquitetura" },
                    new { TopicId = 8, Name = "João Git", Role = "DevOps Engineer", Email = "joao.git@vivo.com.br", Phone = "(11) 9999-0011", Department = "DevOps" },
                    new { TopicId = 9, Name = "Camila QA", Role = "QA Lead", Email = "camila.qa@vivo.com.br", Phone = "(11) 9999-0012", Department = "Qualidade" },
                    new { TopicId = 9, Name = "Bruno Teste", Role = "Analista de Testes", Email = "bruno.teste@vivo.com.br", Phone = "(11) 9999-0013", Department = "Qualidade" },
                    new { TopicId = 10, Name = "Sandra Deploy", Role = "DevOps Lead", Email = "sandra.deploy@vivo.com.br", Phone = "(11) 9999-0014", Department = "DevOps" },
                    new { TopicId = 10, Name = "Miguel Cloud", Role = "Cloud Specialist", Email = "miguel.cloud@vivo.com.br", Phone = "(11) 9999-0015", Department = "Infraestrutura" },
                    new { TopicId = 11, Name = "Cristina Monitor", Role = "SRE Engineer", Email = "cristina.monitor@vivo.com.br", Phone = "(11) 9999-0016", Department = "SRE" },
                    new { TopicId = 12, Name = "Eduardo Doc", Role = "Technical Writer", Email = "eduardo.doc@vivo.com.br", Phone = "(11) 9999-0017", Department = "Documentação" }
                };

                // Inserir documentos
                foreach (var doc in documents)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO topicdocuments (TopicId, Title, Type, Url, Size) VALUES ({0}, {1}, {2}, {3}, {4})",
                        doc.TopicId, doc.Title, doc.Type, doc.Url, doc.Size);
                }
                results.Add($"📄 {documents.Length} documentos inseridos");

                // Inserir links
                foreach (var link in links)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO topiclinks (TopicId, Title, Url) VALUES ({0}, {1}, {2})",
                        link.TopicId, link.Title, link.Url);
                }
                results.Add($"🔗 {links.Length} links inseridos");

                // Inserir contatos
                foreach (var contact in contacts)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO topiccontacts (TopicId, Name, Role, Email, Phone, Department) VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                        contact.TopicId, contact.Name, contact.Role, contact.Email, contact.Phone, contact.Department);
                }
                results.Add($"👥 {contacts.Length} contatos inseridos");

                results.Add("🎉 Conteúdo dos tópicos populado com sucesso!");

                return Ok(new {
                    success = true,
                    message = "Topic content populated successfully",
                    details = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error populating topic content: {ex.Message}");
            }
        }

        [HttpPost("fix-member-progress-table")]
        public async Task<ActionResult> FixMemberProgressTable()
        {
            try
            {
                var results = new List<string>();
                results.Add("🔧 Fixing member_progresses table structure...");

                // Check existing columns
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                // Get current table structure
                var command = connection.CreateCommand();
                command.CommandText = "DESCRIBE member_progresses";

                var existingColumns = new List<string>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingColumns.Add(reader.GetString(0)); // Field name is in first column
                    }
                }
                await connection.CloseAsync();

                results.Add($"📋 Existing columns: {string.Join(", ", existingColumns)}");

                // Add missing columns
                var columnsToAdd = new Dictionary<string, string>
                {
                    {"started_date", "ADD COLUMN started_date DATETIME DEFAULT CURRENT_TIMESTAMP"},
                    {"completed_date", "ADD COLUMN completed_date DATETIME NULL"},
                    {"time_spent", "ADD COLUMN time_spent VARCHAR(20) NULL"},
                    {"notes", "ADD COLUMN notes TEXT NULL"}
                };

                foreach (var column in columnsToAdd)
                {
                    if (!existingColumns.Contains(column.Key))
                    {
                        try
                        {
                            await _context.Database.ExecuteSqlRawAsync($"ALTER TABLE member_progresses {column.Value}");
                            results.Add($"✅ Added column: {column.Key}");
                        }
                        catch (Exception ex)
                        {
                            results.Add($"❌ Failed to add {column.Key}: {ex.Message}");
                        }
                    }
                    else
                    {
                        results.Add($"ℹ️ Column {column.Key} already exists");
                    }
                }

                // Fix column types if needed
                try
                {
                    await _context.Database.ExecuteSqlRawAsync(@"
                        ALTER TABLE member_progresses
                        MODIFY COLUMN user_id VARCHAR(255) NOT NULL,
                        MODIFY COLUMN topic_id INT NOT NULL,
                        MODIFY COLUMN is_completed BOOLEAN DEFAULT FALSE
                    ");
                    results.Add("✅ Updated column types");
                }
                catch (Exception ex)
                {
                    results.Add($"ℹ️ Column types update: {ex.Message}");
                }

                results.Add("🎉 member_progresses table structure fixed!");

                return Ok(new {
                    success = true,
                    message = "member_progresses table structure fixed",
                    details = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fixing member_progresses table: {ex.Message}");
            }
        }

        [HttpPost("remove-test-topics")]
        public async Task<ActionResult> RemoveTestTopics()
        {
            try
            {
                var results = new List<string>();
                results.Add("🗑️ Removendo tópicos de teste...");

                // Primeiro, remover progresso relacionado aos tópicos de teste
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM member_progresses WHERE TopicId IN (1, 2)"
                );
                results.Add("🧹 Progresso dos tópicos de teste removido");

                // Remover documentos, links e contatos dos tópicos de teste (se existirem)
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM topicdocuments WHERE TopicId IN (1, 2)"
                );
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM topiclinks WHERE TopicId IN (1, 2)"
                );
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM topiccontacts WHERE TopicId IN (1, 2)"
                );
                results.Add("🧹 Conteúdo dos tópicos de teste removido");

                // Remover os tópicos de teste (IDs 1 e 2)
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM topics WHERE Id IN (1, 2) AND Title = 'Tópico de Teste'"
                );
                results.Add("✅ Tópicos de teste removidos (IDs 1 e 2)");

                // Verificar quantos tópicos restaram
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM topics";
                var remainingTopics = await command.ExecuteScalarAsync();
                await connection.CloseAsync();

                results.Add($"📊 Tópicos restantes: {remainingTopics}");
                results.Add("🎉 Limpeza concluída com sucesso!");

                return Ok(new {
                    success = true,
                    message = "Test topics removed successfully",
                    details = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error removing test topics: {ex.Message}");
            }
        }

        [HttpPost("create-area-specific-topics")]
        public async Task<ActionResult> CreateAreaSpecificTopics()
        {
            try
            {
                var results = new List<string>();
                results.Add("🎯 Criando tópicos específicos para cada área...");

                // Primeiro, remover tópicos genéricos existentes
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM topicdocuments WHERE TopicId IN (3,4,5,6,7,8,9,10,11,12)"
                );
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM topiclinks WHERE TopicId IN (3,4,5,6,7,8,9,10,11,12)"
                );
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM topiccontacts WHERE TopicId IN (3,4,5,6,7,8,9,10,11,12)"
                );
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM member_progresses WHERE TopicId IN (3,4,5,6,7,8,9,10,11,12)"
                );
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM topics WHERE Id IN (3,4,5,6,7,8,9,10,11,12)"
                );
                results.Add("🧹 Tópicos genéricos removidos");

                // Definir tópicos por área (5 por área = 35 total)
                var topicsByArea = new[]
                {
                    // DESENVOLVIMENTO (IDs 1-5)
                    new { Id = 1, Title = "Fundamentos de .NET Core", Description = "Conceitos básicos e arquitetura do .NET Core", Category = "Desenvolvimento", EstimatedTime = "4h", Tag = "desenvolvimento" },
                    new { Id = 2, Title = "Clean Architecture e SOLID", Description = "Princípios de arquitetura limpa e boas práticas", Category = "Desenvolvimento", EstimatedTime = "3h", Tag = "desenvolvimento" },
                    new { Id = 3, Title = "Entity Framework e Banco de Dados", Description = "ORM, migrations e otimização de queries", Category = "Desenvolvimento", EstimatedTime = "3h", Tag = "desenvolvimento" },
                    new { Id = 4, Title = "Testes Unitários e TDD", Description = "Práticas de teste e desenvolvimento orientado a testes", Category = "Desenvolvimento", EstimatedTime = "2h", Tag = "desenvolvimento" },
                    new { Id = 5, Title = "APIs RESTful e Documentação", Description = "Criação de APIs e documentação com Swagger", Category = "Desenvolvimento", EstimatedTime = "2h", Tag = "desenvolvimento" },

                    // INFRAESTRUTURA (IDs 6-10)
                    new { Id = 6, Title = "Docker e Containerização", Description = "Criação e gerenciamento de containers", Category = "Infraestrutura", EstimatedTime = "3h", Tag = "infraestrutura" },
                    new { Id = 7, Title = "Kubernetes e Orquestração", Description = "Deploy e gerenciamento de aplicações em K8s", Category = "Infraestrutura", EstimatedTime = "4h", Tag = "infraestrutura" },
                    new { Id = 8, Title = "CI/CD com Azure DevOps", Description = "Pipelines de integração e entrega contínua", Category = "Infraestrutura", EstimatedTime = "3h", Tag = "infraestrutura" },
                    new { Id = 9, Title = "Monitoramento e Observabilidade", Description = "Logs, métricas e traces com Grafana/Prometheus", Category = "Infraestrutura", EstimatedTime = "2h", Tag = "infraestrutura" },
                    new { Id = 10, Title = "Segurança em Cloud", Description = "Boas práticas de segurança em ambiente cloud", Category = "Infraestrutura", EstimatedTime = "2h", Tag = "infraestrutura" },

                    // DADOS (IDs 11-15)
                    new { Id = 11, Title = "SQL Server Avançado", Description = "Otimização de queries e performance tuning", Category = "Dados", EstimatedTime = "3h", Tag = "dados" },
                    new { Id = 12, Title = "Power BI e Visualização", Description = "Criação de dashboards e relatórios analíticos", Category = "Dados", EstimatedTime = "3h", Tag = "dados" },
                    new { Id = 13, Title = "Python para Análise de Dados", Description = "Pandas, NumPy e análise exploratória", Category = "Dados", EstimatedTime = "4h", Tag = "dados" },
                    new { Id = 14, Title = "Data Pipeline e ETL", Description = "Construção de pipelines de dados", Category = "Dados", EstimatedTime = "3h", Tag = "dados" },
                    new { Id = 15, Title = "Machine Learning Básico", Description = "Conceitos fundamentais de ML e IA", Category = "Dados", EstimatedTime = "4h", Tag = "dados" },

                    // PRODUTO (IDs 16-20)
                    new { Id = 16, Title = "Product Discovery", Description = "Técnicas de descoberta e validação de produtos", Category = "Produto", EstimatedTime = "2h", Tag = "produto" },
                    new { Id = 17, Title = "User Stories e Backlog", Description = "Escrita de histórias de usuário e gestão de backlog", Category = "Produto", EstimatedTime = "2h", Tag = "produto" },
                    new { Id = 18, Title = "Métricas de Produto", Description = "KPIs, OKRs e análise de performance", Category = "Produto", EstimatedTime = "2h", Tag = "produto" },
                    new { Id = 19, Title = "UX Research e Testes", Description = "Pesquisa com usuários e testes de usabilidade", Category = "Produto", EstimatedTime = "3h", Tag = "produto" },
                    new { Id = 20, Title = "Roadmap e Estratégia", Description = "Planejamento estratégico de produto", Category = "Produto", EstimatedTime = "2h", Tag = "produto" },

                    // QA (IDs 21-25)
                    new { Id = 21, Title = "Testes Manuais e Exploratórios", Description = "Técnicas de teste manual e exploração", Category = "QA", EstimatedTime = "2h", Tag = "qa" },
                    new { Id = 22, Title = "Automação com Selenium", Description = "Testes automatizados de interface", Category = "QA", EstimatedTime = "3h", Tag = "qa" },
                    new { Id = 23, Title = "Testes de API", Description = "Validação de APIs com Postman e RestAssured", Category = "QA", EstimatedTime = "2h", Tag = "qa" },
                    new { Id = 24, Title = "Performance Testing", Description = "Testes de carga e performance", Category = "QA", EstimatedTime = "3h", Tag = "qa" },
                    new { Id = 25, Title = "Quality Assurance Strategy", Description = "Estratégias e processos de qualidade", Category = "QA", EstimatedTime = "2h", Tag = "qa" },

                    // GESTÃO (IDs 26-30)
                    new { Id = 26, Title = "Metodologias Ágeis", Description = "Scrum, Kanban e frameworks ágeis", Category = "Gestão", EstimatedTime = "2h", Tag = "gestao" },
                    new { Id = 27, Title = "Liderança e Pessoas", Description = "Gestão de equipes e desenvolvimento de pessoas", Category = "Gestão", EstimatedTime = "3h", Tag = "gestao" },
                    new { Id = 28, Title = "Gestão de Projetos", Description = "PMI, metodologias e ferramentas de gestão", Category = "Gestão", EstimatedTime = "3h", Tag = "gestao" },
                    new { Id = 29, Title = "Comunicação e Feedback", Description = "Técnicas de comunicação e cultura de feedback", Category = "Gestão", EstimatedTime = "2h", Tag = "gestao" },
                    new { Id = 30, Title = "OKRs e Estratégia", Description = "Definição de objetivos e resultados-chave", Category = "Gestão", EstimatedTime = "2h", Tag = "gestao" },

                    // DESIGN (IDs 31-35)
                    new { Id = 31, Title = "UI/UX Fundamentals", Description = "Princípios de design de interface e experiência", Category = "Design", EstimatedTime = "3h", Tag = "design" },
                    new { Id = 32, Title = "Figma e Prototipação", Description = "Criação de protótipos e design systems", Category = "Design", EstimatedTime = "3h", Tag = "design" },
                    new { Id = 33, Title = "Design Thinking", Description = "Metodologia de design centrado no usuário", Category = "Design", EstimatedTime = "2h", Tag = "design" },
                    new { Id = 34, Title = "Acessibilidade Digital", Description = "Princípios de design inclusivo e acessível", Category = "Design", EstimatedTime = "2h", Tag = "design" },
                    new { Id = 35, Title = "Visual Design e Branding", Description = "Identidade visual e consistência de marca", Category = "Design", EstimatedTime = "2h", Tag = "design" }
                };

                // Inserir todos os tópicos
                foreach (var topic in topicsByArea)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO topics (Id, Title, Description, Category, EstimatedTime, IsActive) VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                        topic.Id, topic.Title, topic.Description, topic.Category, topic.EstimatedTime, true);
                }

                results.Add($"✅ {topicsByArea.Length} tópicos específicos criados");
                results.Add("📋 Áreas cobertas:");
                results.Add("   • Desenvolvimento (5 tópicos)");
                results.Add("   • Infraestrutura (5 tópicos)");
                results.Add("   • Dados (5 tópicos)");
                results.Add("   • Produto (5 tópicos)");
                results.Add("   • QA (5 tópicos)");
                results.Add("   • Gestão (5 tópicos)");
                results.Add("   • Design (5 tópicos)");

                return Ok(new {
                    success = true,
                    message = "Area-specific topics created successfully",
                    details = results,
                    totalTopics = topicsByArea.Length
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating area-specific topics: {ex.Message}");
            }
        }

        [HttpPost("populate-area-specific-content")]
        public async Task<ActionResult> PopulateAreaSpecificContent()
        {
            try
            {
                var results = new List<string>();
                results.Add("📚 Populando conteúdo específico para cada área...");

                // Primeiro, limpar conteúdo existente
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM topicdocuments");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM topiclinks");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM topiccontacts");
                results.Add("🧹 Conteúdo anterior removido");

                var documentCount = 0;
                var linkCount = 0;
                var contactCount = 0;

                // DESENVOLVIMENTO (IDs 1-5)
                var devDocs = new[]
                {
                    new { TopicId = 1, Title = ".NET Core Documentation", Type = "pdf", Url = "/docs/dotnet-core-guide.pdf", Size = "3.2MB" },
                    new { TopicId = 1, Title = "ASP.NET Core Tutorial", Type = "pdf", Url = "/docs/aspnet-tutorial.pdf", Size = "2.8MB" },
                    new { TopicId = 2, Title = "Clean Architecture Guidelines", Type = "pdf", Url = "/docs/clean-architecture.pdf", Size = "1.9MB" },
                    new { TopicId = 2, Title = "SOLID Principles", Type = "doc", Url = "/docs/solid-principles.docx", Size = "1.2MB" },
                    new { TopicId = 3, Title = "Entity Framework Guide", Type = "pdf", Url = "/docs/ef-guide.pdf", Size = "2.4MB" },
                    new { TopicId = 4, Title = "Unit Testing Best Practices", Type = "pdf", Url = "/docs/unit-testing.pdf", Size = "1.8MB" },
                    new { TopicId = 5, Title = "API Design Guidelines", Type = "pdf", Url = "/docs/api-design.pdf", Size = "2.1MB" }
                };

                var devLinks = new[]
                {
                    new { TopicId = 1, Title = "Microsoft .NET Documentation", Url = "https://docs.microsoft.com/dotnet" },
                    new { TopicId = 2, Title = "Clean Code Book", Url = "https://cleancoders.com" },
                    new { TopicId = 3, Title = "Entity Framework Documentation", Url = "https://docs.microsoft.com/ef" },
                    new { TopicId = 4, Title = "xUnit Testing Framework", Url = "https://xunit.net" },
                    new { TopicId = 5, Title = "Swagger/OpenAPI", Url = "https://swagger.io" }
                };

                var devContacts = new[]
                {
                    new { TopicId = 1, Name = "Lucas Desenvolvedor", Role = "Tech Lead .NET", Email = "lucas.dev@vivo.com.br", Phone = "(11) 9999-1001", Department = "Desenvolvimento" },
                    new { TopicId = 2, Name = "Maria Arquiteta", Role = "Software Architect", Email = "maria.arch@vivo.com.br", Phone = "(11) 9999-1002", Department = "Desenvolvimento" },
                    new { TopicId = 3, Name = "João Database", Role = "Database Developer", Email = "joao.db@vivo.com.br", Phone = "(11) 9999-1003", Department = "Desenvolvimento" }
                };

                // INFRAESTRUTURA (IDs 6-10)
                var infraDocs = new[]
                {
                    new { TopicId = 6, Title = "Docker Container Guide", Type = "pdf", Url = "/docs/docker-guide.pdf", Size = "2.9MB" },
                    new { TopicId = 7, Title = "Kubernetes Deployment", Type = "pdf", Url = "/docs/k8s-deployment.pdf", Size = "3.5MB" },
                    new { TopicId = 8, Title = "Azure DevOps Pipelines", Type = "pdf", Url = "/docs/azure-pipelines.pdf", Size = "2.7MB" },
                    new { TopicId = 9, Title = "Grafana Monitoring Setup", Type = "pdf", Url = "/docs/grafana-setup.pdf", Size = "2.2MB" },
                    new { TopicId = 10, Title = "Cloud Security Best Practices", Type = "pdf", Url = "/docs/cloud-security.pdf", Size = "2.6MB" }
                };

                var infraLinks = new[]
                {
                    new { TopicId = 6, Title = "Docker Hub", Url = "https://hub.docker.com" },
                    new { TopicId = 7, Title = "Kubernetes Documentation", Url = "https://kubernetes.io/docs" },
                    new { TopicId = 8, Title = "Azure DevOps", Url = "https://dev.azure.com" },
                    new { TopicId = 9, Title = "Grafana Labs", Url = "https://grafana.com" },
                    new { TopicId = 10, Title = "Azure Security Center", Url = "https://azure.microsoft.com/security" }
                };

                var infraContacts = new[]
                {
                    new { TopicId = 6, Name = "Pedro DevOps", Role = "DevOps Engineer", Email = "pedro.devops@vivo.com.br", Phone = "(11) 9999-2001", Department = "Infraestrutura" },
                    new { TopicId = 7, Name = "Ana Kubernetes", Role = "Cloud Architect", Email = "ana.k8s@vivo.com.br", Phone = "(11) 9999-2002", Department = "Infraestrutura" },
                    new { TopicId = 8, Name = "Carlos Pipeline", Role = "CI/CD Specialist", Email = "carlos.cicd@vivo.com.br", Phone = "(11) 9999-2003", Department = "Infraestrutura" }
                };

                // DADOS (IDs 11-15)
                var dadosDocs = new[]
                {
                    new { TopicId = 11, Title = "SQL Server Performance Guide", Type = "pdf", Url = "/docs/sql-performance.pdf", Size = "3.1MB" },
                    new { TopicId = 12, Title = "Power BI Dashboard Creation", Type = "pdf", Url = "/docs/powerbi-dashboards.pdf", Size = "2.8MB" },
                    new { TopicId = 13, Title = "Python Data Analysis Cookbook", Type = "pdf", Url = "/docs/python-analysis.pdf", Size = "4.2MB" },
                    new { TopicId = 14, Title = "ETL Pipeline Design", Type = "pdf", Url = "/docs/etl-design.pdf", Size = "2.5MB" },
                    new { TopicId = 15, Title = "Machine Learning Fundamentals", Type = "pdf", Url = "/docs/ml-fundamentals.pdf", Size = "3.8MB" }
                };

                var dadosLinks = new[]
                {
                    new { TopicId = 11, Title = "SQL Server Documentation", Url = "https://docs.microsoft.com/sql" },
                    new { TopicId = 12, Title = "Power BI Learning", Url = "https://powerbi.microsoft.com/learning" },
                    new { TopicId = 13, Title = "Pandas Documentation", Url = "https://pandas.pydata.org" },
                    new { TopicId = 14, Title = "Apache Airflow", Url = "https://airflow.apache.org" },
                    new { TopicId = 15, Title = "Scikit-learn", Url = "https://scikit-learn.org" }
                };

                var dadosContacts = new[]
                {
                    new { TopicId = 11, Name = "Rita Database", Role = "Database Analyst", Email = "rita.db@vivo.com.br", Phone = "(11) 9999-3001", Department = "Dados" },
                    new { TopicId = 12, Name = "Felipe BI", Role = "BI Developer", Email = "felipe.bi@vivo.com.br", Phone = "(11) 9999-3002", Department = "Dados" },
                    new { TopicId = 13, Name = "Sofia Python", Role = "Data Scientist", Email = "sofia.python@vivo.com.br", Phone = "(11) 9999-3003", Department = "Dados" }
                };

                // PRODUTO (IDs 16-20)
                var produtoDocs = new[]
                {
                    new { TopicId = 16, Title = "Product Discovery Framework", Type = "pdf", Url = "/docs/product-discovery.pdf", Size = "2.3MB" },
                    new { TopicId = 17, Title = "User Story Writing Guide", Type = "pdf", Url = "/docs/user-stories.pdf", Size = "1.8MB" },
                    new { TopicId = 18, Title = "Product Metrics Handbook", Type = "pdf", Url = "/docs/product-metrics.pdf", Size = "2.1MB" },
                    new { TopicId = 19, Title = "UX Research Methods", Type = "pdf", Url = "/docs/ux-research.pdf", Size = "2.9MB" },
                    new { TopicId = 20, Title = "Product Roadmap Templates", Type = "doc", Url = "/docs/roadmap-templates.docx", Size = "1.5MB" }
                };

                var produtoLinks = new[]
                {
                    new { TopicId = 16, Title = "Product School", Url = "https://productschool.com" },
                    new { TopicId = 17, Title = "User Story Map", Url = "https://www.jpattonassociates.com" },
                    new { TopicId = 18, Title = "Amplitude Analytics", Url = "https://amplitude.com" },
                    new { TopicId = 19, Title = "Nielsen Norman Group", Url = "https://www.nngroup.com" },
                    new { TopicId = 20, Title = "ProductPlan", Url = "https://productplan.com" }
                };

                var produtoContacts = new[]
                {
                    new { TopicId = 16, Name = "Amanda Product", Role = "Product Manager", Email = "amanda.product@vivo.com.br", Phone = "(11) 9999-4001", Department = "Produto" },
                    new { TopicId = 17, Name = "Bruno Stories", Role = "Product Owner", Email = "bruno.po@vivo.com.br", Phone = "(11) 9999-4002", Department = "Produto" },
                    new { TopicId = 18, Name = "Carla Metrics", Role = "Product Analyst", Email = "carla.metrics@vivo.com.br", Phone = "(11) 9999-4003", Department = "Produto" }
                };

                // QA (IDs 21-25)
                var qaDocs = new[]
                {
                    new { TopicId = 21, Title = "Manual Testing Checklist", Type = "pdf", Url = "/docs/manual-testing.pdf", Size = "2.0MB" },
                    new { TopicId = 22, Title = "Selenium WebDriver Guide", Type = "pdf", Url = "/docs/selenium-guide.pdf", Size = "3.1MB" },
                    new { TopicId = 23, Title = "API Testing with Postman", Type = "pdf", Url = "/docs/api-testing.pdf", Size = "2.4MB" },
                    new { TopicId = 24, Title = "Performance Testing Strategy", Type = "pdf", Url = "/docs/performance-testing.pdf", Size = "2.7MB" },
                    new { TopicId = 25, Title = "QA Process Documentation", Type = "pdf", Url = "/docs/qa-process.pdf", Size = "2.2MB" }
                };

                var qaLinks = new[]
                {
                    new { TopicId = 21, Title = "Software Testing Help", Url = "https://www.softwaretestinghelp.com" },
                    new { TopicId = 22, Title = "Selenium Documentation", Url = "https://selenium-python.readthedocs.io" },
                    new { TopicId = 23, Title = "Postman Learning", Url = "https://learning.postman.com" },
                    new { TopicId = 24, Title = "JMeter Documentation", Url = "https://jmeter.apache.org" },
                    new { TopicId = 25, Title = "ISTQB Certification", Url = "https://www.istqb.org" }
                };

                var qaContacts = new[]
                {
                    new { TopicId = 21, Name = "Diana Testing", Role = "QA Lead", Email = "diana.qa@vivo.com.br", Phone = "(11) 9999-5001", Department = "QA" },
                    new { TopicId = 22, Name = "Eduardo Automation", Role = "Test Automation Engineer", Email = "eduardo.auto@vivo.com.br", Phone = "(11) 9999-5002", Department = "QA" },
                    new { TopicId = 23, Name = "Fernanda API", Role = "API Test Specialist", Email = "fernanda.api@vivo.com.br", Phone = "(11) 9999-5003", Department = "QA" }
                };

                // GESTÃO (IDs 26-30)
                var gestaoDoc = new[]
                {
                    new { TopicId = 26, Title = "Agile Methodologies Guide", Type = "pdf", Url = "/docs/agile-guide.pdf", Size = "2.6MB" },
                    new { TopicId = 27, Title = "Leadership Handbook", Type = "pdf", Url = "/docs/leadership.pdf", Size = "3.0MB" },
                    new { TopicId = 28, Title = "Project Management Templates", Type = "doc", Url = "/docs/pm-templates.docx", Size = "1.8MB" },
                    new { TopicId = 29, Title = "Communication Best Practices", Type = "pdf", Url = "/docs/communication.pdf", Size = "2.1MB" },
                    new { TopicId = 30, Title = "OKR Implementation Guide", Type = "pdf", Url = "/docs/okr-guide.pdf", Size = "2.4MB" }
                };

                var gestaoLinks = new[]
                {
                    new { TopicId = 26, Title = "Scrum.org", Url = "https://www.scrum.org" },
                    new { TopicId = 27, Title = "Harvard Business Review", Url = "https://hbr.org" },
                    new { TopicId = 28, Title = "PMI", Url = "https://www.pmi.org" },
                    new { TopicId = 29, Title = "TED Talks Leadership", Url = "https://www.ted.com/topics/leadership" },
                    new { TopicId = 30, Title = "Weekdone OKR", Url = "https://weekdone.com" }
                };

                var gestaoContacts = new[]
                {
                    new { TopicId = 26, Name = "Gustavo Agile", Role = "Agile Coach", Email = "gustavo.agile@vivo.com.br", Phone = "(11) 9999-6001", Department = "Gestão" },
                    new { TopicId = 27, Name = "Helena Leader", Role = "People Manager", Email = "helena.leader@vivo.com.br", Phone = "(11) 9999-6002", Department = "Gestão" },
                    new { TopicId = 28, Name = "Igor Projects", Role = "Project Manager", Email = "igor.pm@vivo.com.br", Phone = "(11) 9999-6003", Department = "Gestão" }
                };

                // DESIGN (IDs 31-35)
                var designDocs = new[]
                {
                    new { TopicId = 31, Title = "UI/UX Design Principles", Type = "pdf", Url = "/docs/ui-ux-principles.pdf", Size = "2.8MB" },
                    new { TopicId = 32, Title = "Figma Design System", Type = "pdf", Url = "/docs/figma-system.pdf", Size = "3.2MB" },
                    new { TopicId = 33, Title = "Design Thinking Toolkit", Type = "pdf", Url = "/docs/design-thinking.pdf", Size = "2.5MB" },
                    new { TopicId = 34, Title = "Accessibility Guidelines", Type = "pdf", Url = "/docs/accessibility.pdf", Size = "2.1MB" },
                    new { TopicId = 35, Title = "Brand Style Guide", Type = "pdf", Url = "/docs/brand-guide.pdf", Size = "3.4MB" }
                };

                var designLinks = new[]
                {
                    new { TopicId = 31, Title = "Material Design", Url = "https://material.io" },
                    new { TopicId = 32, Title = "Figma Community", Url = "https://www.figma.com/community" },
                    new { TopicId = 33, Title = "IDEO Design Kit", Url = "https://www.designkit.org" },
                    new { TopicId = 34, Title = "Web Accessibility", Url = "https://www.w3.org/WAI" },
                    new { TopicId = 35, Title = "Adobe Color", Url = "https://color.adobe.com" }
                };

                var designContacts = new[]
                {
                    new { TopicId = 31, Name = "Julia Designer", Role = "UX Designer", Email = "julia.ux@vivo.com.br", Phone = "(11) 9999-7001", Department = "Design" },
                    new { TopicId = 32, Name = "Leonardo Figma", Role = "UI Designer", Email = "leonardo.ui@vivo.com.br", Phone = "(11) 9999-7002", Department = "Design" },
                    new { TopicId = 33, Name = "Mariana Thinking", Role = "Design Researcher", Email = "mariana.research@vivo.com.br", Phone = "(11) 9999-7003", Department = "Design" }
                };

                // Inserir todos os documentos
                foreach (var doc in devDocs.Concat(infraDocs).Concat(dadosDocs).Concat(produtoDocs).Concat(qaDocs).Concat(gestaoDoc).Concat(designDocs))
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO topicdocuments (TopicId, Title, Type, Url, Size) VALUES ({0}, {1}, {2}, {3}, {4})",
                        doc.TopicId, doc.Title, doc.Type, doc.Url, doc.Size);
                    documentCount++;
                }

                // Inserir todos os links
                foreach (var link in devLinks.Concat(infraLinks).Concat(dadosLinks).Concat(produtoLinks).Concat(qaLinks).Concat(gestaoLinks).Concat(designLinks))
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO topiclinks (TopicId, Title, Url) VALUES ({0}, {1}, {2})",
                        link.TopicId, link.Title, link.Url);
                    linkCount++;
                }

                // Inserir todos os contatos
                foreach (var contact in devContacts.Concat(infraContacts).Concat(dadosContacts).Concat(produtoContacts).Concat(qaContacts).Concat(gestaoContacts).Concat(designContacts))
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO topiccontacts (TopicId, Name, Role, Email, Phone, Department) VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                        contact.TopicId, contact.Name, contact.Role, contact.Email, contact.Phone, contact.Department);
                    contactCount++;
                }

                results.Add($"📄 {documentCount} documentos específicos inseridos");
                results.Add($"🔗 {linkCount} links específicos inseridos");
                results.Add($"👥 {contactCount} contatos específicos inseridos");
                results.Add("🎉 Conteúdo específico por área populado com sucesso!");

                return Ok(new {
                    success = true,
                    message = "Area-specific content populated successfully",
                    details = results,
                    counts = new { documents = documentCount, links = linkCount, contacts = contactCount }
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error populating area-specific content: {ex.Message}");
            }
        }

        [HttpPost("update-document-urls")]
        public async Task<ActionResult> UpdateDocumentUrls()
        {
            try
            {
                // Atualizar todas as URLs dos documentos para usar arquivos reais
                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE documents
                    SET url = 'exemplo-documento.pdf'
                    WHERE type = 'pdf'
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE documents
                    SET url = 'exemplo-documento.pdf'
                    WHERE type = 'doc'
                ");

                return Ok(new {
                    success = true,
                    message = "URLs dos documentos atualizadas para arquivos reais"
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao atualizar URLs: {ex.Message}");
            }
        }

        [HttpPost("update-dotnet-core-pdfs")]
        public async Task<ActionResult> UpdateDotNetCorePdfs()
        {
            try
            {
                // Primeiro, vamos encontrar o ID do tópico "Fundamentos de .NET Core"
                var topicId = await _context.Database.ExecuteSqlRawAsync(@"
                    SELECT id FROM topics WHERE title LIKE '%Fundamentos de .NET Core%' LIMIT 1
                ");

                // Atualizar documentos específicos do tópico .NET Core
                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE documents
                    SET title = 'Guia .NET Core Documentation', url = 'dotnet-core-documentation.pdf'
                    WHERE topic_id = (SELECT id FROM topics WHERE title LIKE '%Fundamentos de .NET Core%' LIMIT 1)
                    AND title LIKE '%Guia .NET Core%'
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE documents
                    SET title = 'ASP.NET Core Tutorial', url = 'aspnet-core-tutorial.pdf'
                    WHERE topic_id = (SELECT id FROM topics WHERE title LIKE '%Fundamentos de .NET Core%' LIMIT 1)
                    AND title LIKE '%Clean Architecture%'
                ");

                // Se não existirem, vamos inserir os novos documentos
                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT IGNORE INTO documents (topic_id, title, type, url)
                    SELECT id, 'Guia .NET Core Documentation', 'pdf', 'dotnet-core-documentation.pdf'
                    FROM topics
                    WHERE title LIKE '%Fundamentos de .NET Core%'
                    LIMIT 1
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT IGNORE INTO documents (topic_id, title, type, url)
                    SELECT id, 'ASP.NET Core Tutorial', 'pdf', 'aspnet-core-tutorial.pdf'
                    FROM topics
                    WHERE title LIKE '%Fundamentos de .NET Core%'
                    LIMIT 1
                ");

                return Ok(new {
                    success = true,
                    message = "PDFs didáticos do .NET Core atualizados com sucesso!",
                    files = new[] {
                        "dotnet-core-documentation.pdf",
                        "aspnet-core-tutorial.pdf"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao atualizar PDFs do .NET Core: {ex.Message}");
            }
        }

        [HttpPost("create-links-contacts-tables")]
        public async Task<ActionResult> CreateLinksContactsTables()
        {
            try
            {
                var results = new List<string>();
                results.Add("🔧 Criando tabelas links_uteis e contatos_referencia...");

                // Criar tabela links_uteis
                await _context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS links_uteis (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        TopicId INT NOT NULL,
                        Title VARCHAR(200) NOT NULL,
                        Url VARCHAR(500) NOT NULL,
                        INDEX idx_topicid (TopicId),
                        FOREIGN KEY (TopicId) REFERENCES topics(Id) ON DELETE CASCADE
                    )
                ");
                results.Add("✅ Tabela links_uteis criada");

                // Criar tabela contatos_referencia
                await _context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS contatos_referencia (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        TopicId INT NOT NULL,
                        Name VARCHAR(200) NOT NULL,
                        Role VARCHAR(200) NOT NULL,
                        Email VARCHAR(200) NOT NULL,
                        Phone VARCHAR(20),
                        Department VARCHAR(100),
                        INDEX idx_topicid (TopicId),
                        FOREIGN KEY (TopicId) REFERENCES topics(Id) ON DELETE CASCADE
                    )
                ");
                results.Add("✅ Tabela contatos_referencia criada");

                // Popular com alguns dados de exemplo
                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO links_uteis (TopicId, Title, Url) VALUES
                    (1, 'Microsoft .NET Documentation', 'https://docs.microsoft.com/dotnet'),
                    (1, 'C# Programming Guide', 'https://docs.microsoft.com/dotnet/csharp'),
                    (2, 'Clean Architecture Book', 'https://cleancoders.com'),
                    (2, 'SOLID Principles', 'https://blog.cleancoder.com'),
                    (3, 'Entity Framework Core', 'https://docs.microsoft.com/ef'),
                    (3, 'Database Design', 'https://www.postgresql.org/docs')
                ");
                results.Add("✅ Links úteis de exemplo inseridos");

                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO contatos_referencia (TopicId, Name, Role, Email, Phone, Department) VALUES
                    (1, 'Lucas Desenvolvedor', 'Tech Lead .NET', 'lucas.dev@vivo.com.br', '(11) 9999-1001', 'Desenvolvimento'),
                    (1, 'Maria Santos', 'Senior Developer', 'maria.santos@vivo.com.br', '(11) 9999-1002', 'Desenvolvimento'),
                    (2, 'Pedro Arquiteto', 'Software Architect', 'pedro.arch@vivo.com.br', '(11) 9999-2001', 'Desenvolvimento'),
                    (3, 'Ana Database', 'Database Specialist', 'ana.db@vivo.com.br', '(11) 9999-3001', 'Dados')
                ");
                results.Add("✅ Contatos de referência de exemplo inseridos");

                results.Add("🎉 Tabelas criadas e populadas com sucesso!");

                return Ok(new {
                    success = true,
                    message = "Tables created and populated successfully",
                    details = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao criar tabelas: {ex.Message}");
            }
        }
    }
}