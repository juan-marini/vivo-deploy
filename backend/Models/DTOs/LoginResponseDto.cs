namespace backend.Models.DTOs
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public UsuarioDto? Usuario { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool PrimeiroAcesso { get; set; }
        public string? Departamento { get; set; }
        public string? Cargo { get; set; }
    }
}