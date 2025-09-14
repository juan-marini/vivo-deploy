using backend.Models.DTOs;

namespace backend.Services.Interfaces
{
    public interface IProgressService
    {
        Task<MemberDetailedProgressDto?> GetMemberDetailedProgressAsync(string memberId);
        Task<List<TeamMemberDto>> GetAllMembersAsync();
        Task MarkTopicAsCompletedAsync(string memberId, int topicId);
        Task AddActivityAsync(string memberId, ActivityItemDto activity);
        Task<List<ActivityItemDto>> GetMemberActivityAsync(string memberId);
        Task<List<TopicDto>> GetAllTopicsAsync();
        Task<List<MemberTopicProgressDto>> GetMemberTopicsProgressAsync(string memberId);
    }
}