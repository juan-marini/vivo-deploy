using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities
{
    public class TopicDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TopicId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // pdf, doc, link

        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Size { get; set; }

        // Navigation property
        public virtual Topic? Topic { get; set; }
    }
}