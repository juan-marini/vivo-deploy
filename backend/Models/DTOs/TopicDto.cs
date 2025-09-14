namespace backend.Models.DTOs
{
    public class TopicDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public List<TopicDocumentDto> Documents { get; set; } = new();
        public List<TopicLinkDto> Links { get; set; } = new();
        public List<TopicContactDto> Contacts { get; set; } = new();
    }

    public class TopicDocumentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Size { get; set; }
    }

    public class TopicLinkDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class TopicContactDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}