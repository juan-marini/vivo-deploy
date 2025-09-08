using Microsoft.EntityFrameworkCore;
using backend.Models.Entities;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Sessao> Sessoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuração da tabela Perfil
            modelBuilder.Entity<Perfil>().ToTable("perfis");
            
            // Configuração da tabela Usuario
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Perfil)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(u => u.PerfilId);
            
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Gestor)
                .WithMany(u => u.Subordinados)
                .HasForeignKey(u => u.GestorId)
                .IsRequired(false);
            
            // Configuração da tabela Sessao
            modelBuilder.Entity<Sessao>()
                .HasOne(s => s.Usuario)
                .WithMany(u => u.Sessoes)
                .HasForeignKey(s => s.UsuarioId);
        }
    }
}
