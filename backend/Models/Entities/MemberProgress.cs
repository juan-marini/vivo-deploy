using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities
{
    public class MemberProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int TopicId { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; }

        public DateTime StartedDate { get; set; } = DateTime.UtcNow;

        public string? TimeSpent { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        public virtual Usuario? User { get; set; }
        public virtual Topic? Topic { get; set; }
    }
}