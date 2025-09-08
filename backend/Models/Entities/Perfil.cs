namespace backend.Models.Entities
{
    public class Perfil
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;
        
        // Navigation
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}