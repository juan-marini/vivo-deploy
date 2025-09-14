using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using backend.Services.Interfaces;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _progressService;

        public ProgressController(IProgressService progressService)
        {
            _progressService = progressService;
        }

        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<MemberDetailedProgressDto>> GetMemberDetailedProgress(string memberId)
        {
            try
            {
                var result = await _progressService.GetMemberDetailedProgressAsync(memberId);
                if (result == null)
                {
                    return NotFound($"Member with ID {memberId} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving member progress: {ex.Message}");
            }
        }

        [HttpGet("members")]
        public async Task<ActionResult<List<TeamMemberDto>>> GetAllMembers()
        {
            try
            {
                var members = await _progressService.GetAllMembersAsync();
                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving members: {ex.Message}");
            }
        }

        [HttpPost("member/{memberId}/topic/{topicId}/complete")]
        public async Task<ActionResult> MarkTopicAsCompleted(string memberId, int topicId)
        {
            try
            {
                await _progressService.MarkTopicAsCompletedAsync(memberId, topicId);
                return Ok(new { message = "Topic marked as completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error marking topic as completed: {ex.Message}");
            }
        }

        [HttpPost("member/{memberId}/activity")]
        public async Task<ActionResult> AddActivity(string memberId, [FromBody] ActivityItemDto activity)
        {
            try
            {
                await _progressService.AddActivityAsync(memberId, activity);
                return Ok(new { message = "Activity added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error adding activity: {ex.Message}");
            }
        }

        [HttpGet("member/{memberId}/activity")]
        public async Task<ActionResult<List<ActivityItemDto>>> GetMemberActivity(string memberId)
        {
            try
            {
                var activities = await _progressService.GetMemberActivityAsync(memberId);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving member activity: {ex.Message}");
            }
        }

        [HttpGet("topics")]
        public async Task<ActionResult<List<TopicDto>>> GetAllTopics()
        {
            try
            {
                var topics = await _progressService.GetAllTopicsAsync();
                return Ok(topics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving topics: {ex.Message}");
            }
        }
    }
}