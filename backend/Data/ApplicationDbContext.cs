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
        public DbSet<Topic> Topics { get; set; }
        public DbSet<TopicDocument> TopicDocuments { get; set; }
        public DbSet<TopicLink> TopicLinks { get; set; }
        public DbSet<TopicContact> TopicContacts { get; set; }
        public DbSet<MemberProgress> MemberProgresses { get; set; }
        public DbSet<Activity> Activities { get; set; }

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

            // Configuração das tabelas de Topics
            modelBuilder.Entity<Topic>()
                .ToTable("topics");

            modelBuilder.Entity<TopicDocument>()
                .ToTable("topicdocuments");

            modelBuilder.Entity<TopicLink>()
                .ToTable("links_uteis");

            modelBuilder.Entity<TopicContact>()
                .ToTable("contatos_referencia");

            modelBuilder.Entity<Topic>()
                .HasMany(t => t.Documents)
                .WithOne(d => d.Topic)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Topic>()
                .HasMany(t => t.Links)
                .WithOne(l => l.Topic)
                .HasForeignKey(l => l.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Topic>()
                .HasMany(t => t.Contacts)
                .WithOne(c => c.Topic)
                .HasForeignKey(c => c.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Topic>()
                .HasMany(t => t.MemberProgresses)
                .WithOne(mp => mp.Topic)
                .HasForeignKey(mp => mp.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração da tabela MemberProgress
            modelBuilder.Entity<MemberProgress>()
                .ToTable("member_progresses");

            modelBuilder.Entity<MemberProgress>()
                .Property(mp => mp.TopicId)
                .HasColumnName("topic_id");

            modelBuilder.Entity<MemberProgress>()
                .Property(mp => mp.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<MemberProgress>()
                .Property(mp => mp.IsCompleted)
                .HasColumnName("is_completed");

            modelBuilder.Entity<MemberProgress>()
                .Property(mp => mp.CompletedDate)
                .HasColumnName("completed_date");

            modelBuilder.Entity<MemberProgress>()
                .Property(mp => mp.StartedDate)
                .HasColumnName("started_date");

            modelBuilder.Entity<MemberProgress>()
                .Property(mp => mp.TimeSpent)
                .HasColumnName("time_spent");

            modelBuilder.Entity<MemberProgress>()
                .HasOne(mp => mp.User)
                .WithMany()
                .HasForeignKey(mp => mp.UserId)
                .HasPrincipalKey(u => u.Email)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração da tabela Activity
            modelBuilder.Entity<Activity>()
                .ToTable("activities");

            modelBuilder.Entity<Activity>()
                .Property(a => a.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<Activity>()
                .Property(a => a.Action)
                .HasColumnName("action");

            modelBuilder.Entity<Activity>()
                .Property(a => a.TopicTitle)
                .HasColumnName("topic_title");

            modelBuilder.Entity<Activity>()
                .Property(a => a.Type)
                .HasColumnName("type");

            modelBuilder.Entity<Activity>()
                .Property(a => a.Date)
                .HasColumnName("date");

            modelBuilder.Entity<Activity>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .HasPrincipalKey(u => u.Email)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            modelBuilder.Entity<MemberProgress>()
                .HasIndex(mp => new { mp.UserId, mp.TopicId })
                .IsUnique();

            // Activity indices
            modelBuilder.Entity<Activity>()
                .HasIndex(a => new { a.UserId, a.Date });

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
