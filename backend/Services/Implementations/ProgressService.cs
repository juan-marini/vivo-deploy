using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models.DTOs;
using backend.Models.Entities;
using backend.Services.Interfaces;

namespace backend.Services.Implementations
{
    public class ProgressService : IProgressService
    {
        private readonly ApplicationDbContext _context;

        public ProgressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MemberDetailedProgressDto?> GetMemberDetailedProgressAsync(string memberId)
        {
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == memberId);

            if (user == null)
                return null;

            var memberTopicsProgress = await GetMemberTopicsProgressAsync(memberId);

            var totalTopics = memberTopicsProgress.Count;
            var completedTopics = memberTopicsProgress.Count(t => t.IsCompleted);
            var inProgressTopics = memberTopicsProgress.Count(t => !t.IsCompleted && HasStartedTopic(memberId, t.TopicId));
            var notStartedTopics = totalTopics - completedTopics - inProgressTopics;

            // Calculate time estimates
            var totalTimeEstimated = CalculateTotalTime(memberTopicsProgress.Select(t => t.EstimatedTime).ToList());
            var totalTimeSpent = CalculateTimeSpent(memberId, memberTopicsProgress);
            var averageTopicTime = completedTopics > 0 ? CalculateAverageTime(totalTimeSpent, completedTopics) : "0min";

            // Get recent activity
            var recentActivity = await GetMemberActivityAsync(memberId);

            // Get suggested next topics (uncompleted topics in order)
            var suggestedNextTopics = memberTopicsProgress
                .Where(t => !t.IsCompleted)
                .Take(3)
                .ToList();

            var memberDto = new TeamMemberDto
            {
                Id = user.Email,
                Name = user.Nome,
                Role = GetUserRole(user.PerfilId),
                StartDate = user.DataCadastro.ToString("dd/MM/yyyy"),
                Progress = totalTopics > 0 ? (int)Math.Round((double)completedTopics / totalTopics * 100) : 0
            };

            memberDto.Status = CalculateStatusFromProgress(memberDto.Progress);

            return new MemberDetailedProgressDto
            {
                Member = memberDto,
                TopicsProgress = memberTopicsProgress,
                TotalTopics = totalTopics,
                CompletedTopics = completedTopics,
                InProgressTopics = inProgressTopics,
                NotStartedTopics = notStartedTopics,
                TotalTimeEstimated = totalTimeEstimated,
                TotalTimeSpent = totalTimeSpent,
                AverageTopicTime = averageTopicTime,
                RecentActivity = recentActivity.Take(10).ToList(),
                SuggestedNextTopics = suggestedNextTopics
            };
        }

        public async Task<List<TeamMemberDto>> GetAllMembersAsync()
        {
            var users = await _context.Usuarios
                .Where(u => u.PerfilId != 1) // Excluding admin users
                .Select(u => new TeamMemberDto
                {
                    Id = u.Email,
                    Name = u.Nome,
                    Role = GetUserRole(u.PerfilId),
                    StartDate = u.DataCadastro.ToString("dd/MM/yyyy"),
                    Progress = 0,
                    Status = "Não iniciado"
                })
                .ToListAsync();

            // Update progress for each member
            foreach (var member in users)
            {
                var topicsProgress = await GetMemberTopicsProgressAsync(member.Id);
                var completedTopics = topicsProgress.Count(t => t.IsCompleted);
                var totalTopics = topicsProgress.Count;

                member.Progress = totalTopics > 0 ? (int)Math.Round((double)completedTopics / totalTopics * 100) : 0;
                member.Status = CalculateStatusFromProgress(member.Progress);
            }

            return users;
        }

        public async Task<List<MemberTopicProgressDto>> GetMemberTopicsProgressAsync(string memberId)
        {
            var topics = await _context.Topics
                .Where(t => t.IsActive)
                .OrderBy(t => t.Id)
                .ToListAsync();

            var memberProgress = await _context.MemberProgresses
                .Where(mp => mp.UserId == memberId)
                .ToListAsync();

            return topics.Select(topic =>
            {
                var progress = memberProgress.FirstOrDefault(mp => mp.TopicId == topic.Id);
                return new MemberTopicProgressDto
                {
                    TopicId = topic.Id,
                    TopicTitle = topic.Title,
                    Category = topic.Category,
                    EstimatedTime = topic.EstimatedTime,
                    IsCompleted = progress?.IsCompleted ?? false,
                    CompletedDate = progress?.CompletedDate?.ToString("yyyy-MM-dd"),
                    TimeSpent = progress?.TimeSpent
                };
            }).ToList();
        }

        public async Task MarkTopicAsCompletedAsync(string memberId, int topicId)
        {
            var existing = await _context.MemberProgresses
                .FirstOrDefaultAsync(mp => mp.UserId == memberId && mp.TopicId == topicId);

            if (existing != null)
            {
                existing.IsCompleted = true;
                existing.CompletedDate = DateTime.UtcNow;
                _context.MemberProgresses.Update(existing);
            }
            else
            {
                var newProgress = new MemberProgress
                {
                    UserId = memberId,
                    TopicId = topicId,
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    StartedDate = DateTime.UtcNow
                };
                _context.MemberProgresses.Add(newProgress);
            }

            // Add activity
            var topic = await _context.Topics.FindAsync(topicId);
            if (topic != null)
            {
                var activity = new Activity
                {
                    UserId = memberId,
                    Action = "Concluiu o tópico",
                    TopicTitle = topic.Title,
                    Type = "completed",
                    Date = DateTime.UtcNow
                };
                _context.Activities.Add(activity);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddActivityAsync(string memberId, ActivityItemDto activityDto)
        {
            var activity = new Activity
            {
                UserId = memberId,
                Action = activityDto.Action,
                TopicTitle = activityDto.TopicTitle,
                Type = activityDto.Type,
                Date = DateTime.Parse(activityDto.Date)
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActivityItemDto>> GetMemberActivityAsync(string memberId)
        {
            var activities = await _context.Activities
                .Where(a => a.UserId == memberId)
                .OrderByDescending(a => a.Date)
                .Take(20)
                .Select(a => new ActivityItemDto
                {
                    Date = a.Date.ToString("yyyy-MM-dd"),
                    Action = a.Action,
                    TopicTitle = a.TopicTitle,
                    Type = a.Type
                })
                .ToListAsync();

            return activities;
        }

        public async Task<List<TopicDto>> GetAllTopicsAsync()
        {
            var topics = await _context.Topics
                .Include(t => t.Documents)
                .Include(t => t.Links)
                .Include(t => t.Contacts)
                .Where(t => t.IsActive)
                .Select(t => new TopicDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Category = t.Category,
                    EstimatedTime = t.EstimatedTime,
                    Documents = t.Documents.Select(d => new TopicDocumentDto
                    {
                        Id = d.Id,
                        Title = d.Title,
                        Type = d.Type,
                        Url = d.Url,
                        Size = d.Size
                    }).ToList(),
                    Links = t.Links.Select(l => new TopicLinkDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Url = l.Url
                    }).ToList(),
                    Contacts = t.Contacts.Select(c => new TopicContactDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Role = c.Role,
                        Email = c.Email,
                        Phone = c.Phone,
                        Department = c.Department
                    }).ToList()
                })
                .ToListAsync();

            return topics;
        }

        private bool HasStartedTopic(string userId, int topicId)
        {
            return _context.MemberProgresses
                .Any(mp => mp.UserId == userId && mp.TopicId == topicId && !mp.IsCompleted);
        }

        private string CalculateTotalTime(List<string> estimatedTimes)
        {
            int totalMinutes = 0;
            foreach (var time in estimatedTimes)
            {
                totalMinutes += ParseTimeToMinutes(time);
            }
            return FormatMinutesToTime(totalMinutes);
        }

        private int ParseTimeToMinutes(string time)
        {
            if (string.IsNullOrEmpty(time)) return 0;

            var lowerTime = time.ToLower();
            if (lowerTime.Contains("h"))
            {
                var parts = lowerTime.Split('h');
                if (float.TryParse(parts[0], out float hours))
                {
                    return (int)(hours * 60);
                }
            }
            else if (lowerTime.Contains("min"))
            {
                var parts = lowerTime.Split("min");
                if (int.TryParse(parts[0], out int minutes))
                {
                    return minutes;
                }
            }
            return 0;
        }

        private string FormatMinutesToTime(int minutes)
        {
            if (minutes < 60) return $"{minutes}min";
            int hours = minutes / 60;
            int remainingMinutes = minutes % 60;
            return remainingMinutes > 0 ? $"{hours}h {remainingMinutes}min" : $"{hours}h";
        }

        private string CalculateTimeSpent(string userId, List<MemberTopicProgressDto> topics)
        {
            var completedTopics = topics.Where(t => t.IsCompleted).ToList();
            int totalMinutes = 0;
            foreach (var topic in completedTopics)
            {
                totalMinutes += ParseTimeToMinutes(topic.EstimatedTime);
            }
            return FormatMinutesToTime((int)(totalMinutes * 0.8)); // Assume 80% efficiency
        }

        private string CalculateAverageTime(string totalTime, int completedCount)
        {
            int totalMinutes = ParseTimeToMinutes(totalTime);
            int avgMinutes = totalMinutes / completedCount;
            return FormatMinutesToTime(avgMinutes);
        }

        private string GetUserRole(int perfilId)
        {
            return perfilId switch
            {
                1 => "Gestor",
                2 => "Desenvolvedor Backend",
                3 => "Desenvolvedor Frontend",
                4 => "Analista de Dados",
                _ => "Colaborador"
            };
        }

        private string CalculateStatusFromProgress(int progress)
        {
            return progress switch
            {
                100 => "Concluído",
                0 => "Não iniciado",
                < 50 => "Atrasado",
                _ => "Em andamento"
            };
        }
    }
}