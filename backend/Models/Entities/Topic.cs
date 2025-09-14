using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities
{
    public class Topic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(20)]
        public string EstimatedTime { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<MemberProgress> MemberProgresses { get; set; } = new List<MemberProgress>();
        public virtual ICollection<TopicDocument> Documents { get; set; } = new List<TopicDocument>();
        public virtual ICollection<TopicLink> Links { get; set; } = new List<TopicLink>();
        public virtual ICollection<TopicContact> Contacts { get; set; } = new List<TopicContact>();
    }
}