using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateUserTaskDto
    {
        [Required(ErrorMessage = "Task name is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Task name must be between 1 and 255 characters")]
        public string TaskName { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Task description cannot exceed 1000 characters")]
        public string? TaskDescription { get; set; }

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Local time is required")]
        public TimeOnly LocalTime { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public int? RecurrenceId { get; set; }
    }

    public class UpdateUserTaskDto
    {
        [Required(ErrorMessage = "User task ID is required")]
        public int UserTaskId { get; set; }

        [Required(ErrorMessage = "Task name is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Task name must be between 1 and 255 characters")]
        public string TaskName { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Task description cannot exceed 1000 characters")]
        public string? TaskDescription { get; set; }

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Local time is required")]
        public TimeOnly LocalTime { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public int? RecurrenceId { get; set; }

        [Required(ErrorMessage = "IsDeleted status is required")]
        public bool IsDeleted { get; set; }
    }

    public class UserTaskResponseDto
    {
        public int UserTaskId { get; set; }

        public string TaskName { get; set; } = null!;

        public string? TaskDescription { get; set; }

        public string? ImageUrl { get; set; }

        public TimeOnly LocalTime { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public int UserId { get; set; }

        public int? RecurrenceId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public UserResponseDto? User { get; set; }

        public TaskRecurrenceResponseDto? Recurrence { get; set; }

        public ICollection<TaskStepResponseDto> TaskSteps { get; set; } = new List<TaskStepResponseDto>();
    }
}
