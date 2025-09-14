namespace backend.Models.DTOs
{
    public class MemberDetailedProgressDto
    {
        public TeamMemberDto Member { get; set; } = new();
        public List<MemberTopicProgressDto> TopicsProgress { get; set; } = new();
        public int TotalTopics { get; set; }
        public int CompletedTopics { get; set; }
        public int InProgressTopics { get; set; }
        public int NotStartedTopics { get; set; }
        public string TotalTimeEstimated { get; set; } = string.Empty;
        public string TotalTimeSpent { get; set; } = string.Empty;
        public string AverageTopicTime { get; set; } = string.Empty;
        public List<ActivityItemDto> RecentActivity { get; set; } = new();
        public List<MemberTopicProgressDto> SuggestedNextTopics { get; set; } = new();
    }

    public class TeamMemberDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public int Progress { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class MemberTopicProgressDto
    {
        public int TopicId { get; set; }
        public string TopicTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string? CompletedDate { get; set; }
        public string? TimeSpent { get; set; }
    }

    public class ActivityItemDto
    {
        public string Date { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string TopicTitle { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}