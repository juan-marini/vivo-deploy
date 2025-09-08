using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Entities
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Senha { get; set; } = string.Empty;
        
        [Required]
        [Column("nome_completo")]
        public string NomeCompleto { get; set; } = string.Empty;
        
        [Column("perfil_id")]
        public int PerfilId { get; set; }
        
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }
        
        public bool Ativo { get; set; } = true;
        
        [Column("primeiro_acesso")]
        public bool PrimeiroAcesso { get; set; } = true;
        
        [Column("data_admissao")]
        public DateTime? DataAdmissao { get; set; }
        
        public string? Telefone { get; set; }
        public string? Departamento { get; set; }
        public string? Cargo { get; set; }
        
        [Column("gestor_id")]
        public int? GestorId { get; set; }
        
        [Column("ultimo_login")]
        public DateTime? UltimoLogin { get; set; }
        
        // Navigation Properties
        [ForeignKey("PerfilId")]
        public virtual Perfil? Perfil { get; set; }
        
        [ForeignKey("GestorId")]
        public virtual Usuario? Gestor { get; set; }
        
        public virtual ICollection<Usuario> Subordinados { get; set; } = new List<Usuario>();
        public virtual ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
    }
}
