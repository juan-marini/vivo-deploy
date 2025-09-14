using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities
{
    public class TopicContact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TopicId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Role { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        // Navigation property
        public virtual Topic? Topic { get; set; }
    }
}