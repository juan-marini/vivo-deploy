using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using System.Text;
using backend.Data;
using backend.Middleware;
using backend.Services.Implementations;
using backend.Services.Interfaces;
using backend.Utils;

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel para usar HTTP em desenvolvimento
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // HTTP
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Vivo Knowledge API", 
        Version = "v1" 
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"📌 Connection String: {connectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    options.UseMySql(connectionString, serverVersion)
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.ASCII.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// CORS será tratado por middleware customizado

var app = builder.Build();

// Teste de conexão e Seed do banco
Console.WriteLine("\n========================================");
Console.WriteLine("🚀 INICIANDO VIVO KNOWLEDGE API");
Console.WriteLine("========================================\n");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    Console.WriteLine("🔍 Testando conexão com MySQL...");
    try
    {
        if (await context.Database.CanConnectAsync())
        {
            Console.WriteLine("✅ Conexão com MySQL estabelecida!");
            
            // Verificar se as tabelas existem
            var temTabelas = await context.Database.ExecuteSqlRawAsync(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'vivo_knowledge_db'");
            
            // Popular dados iniciais
            await DatabaseSeeder.SeedDatabase(app.Services);
        }
        else
        {
            Console.WriteLine("❌ Não foi possível conectar ao MySQL");
            Console.WriteLine("   Verifique se o XAMPP está rodando");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro de conexão: {ex.Message}");
        Console.WriteLine("\n🔧 Checklist de resolução:");
        Console.WriteLine("   1. XAMPP está rodando?");
        Console.WriteLine("   2. MySQL está 'Start' no XAMPP?");
        Console.WriteLine("   3. Banco 'vivo_knowledge_db' foi criado?");
        Console.WriteLine("   4. Script SQL foi executado?");
    }
}

// CORS deve ser o primeiro middleware
app.UseMiddleware<CorsMiddleware>();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vivo Knowledge API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ErrorHandlingMiddleware>();

// Configurar arquivos estáticos para servir PDFs
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files")),
    RequestPath = "/files"
});

app.UseStaticFiles(); // Para outros arquivos estáticos

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Endpoint de teste
app.MapGet("/", () => new
{
    message = "Vivo Knowledge API está rodando!",
    swagger = "http://localhost:5000/swagger",
    timestamp = DateTime.Now
});

Console.WriteLine("\n========================================");
Console.WriteLine("✅ API RODANDO COM SUCESSO!");
Console.WriteLine("========================================");
Console.WriteLine($"🌐 API: http://localhost:5000");
Console.WriteLine($"📚 Swagger: http://localhost:5000/swagger");
Console.WriteLine($"🔐 Teste de login: http://localhost:5000/api/auth/test");
Console.WriteLine("========================================\n");

app.Run();