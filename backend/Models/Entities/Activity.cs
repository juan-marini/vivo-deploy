using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(200)]
        public string TopicTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // completed, started, updated

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Usuario? User { get; set; }
    }
}