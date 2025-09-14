using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities
{
    public class TopicLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TopicId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        // Navigation property
        public virtual Topic? Topic { get; set; }
    }
}