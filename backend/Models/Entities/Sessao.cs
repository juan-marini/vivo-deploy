using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Entities
{
    [Table("sessoes")]
    public class Sessao
    {
        [Key]
        public int Id { get; set; }
        
        [Column("usuario_id")]
        public int UsuarioId { get; set; }
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Column("refresh_token")]
        public string? RefreshToken { get; set; }
        
        [Column("ip_address")]
        public string? IpAddress { get; set; }
        
        [Column("user_agent")]
        public string? UserAgent { get; set; }
        
        [Column("expira_em")]
        public DateTime ExpiraEm { get; set; }
        
        [Column("criado_em")]
        public DateTime CriadoEm { get; set; }
        
        // Navigation
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}