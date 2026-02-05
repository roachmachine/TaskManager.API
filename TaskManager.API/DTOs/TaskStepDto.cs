using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateTaskStepDto
    {
        [Required(ErrorMessage = "User task ID is required")]
        public int UserTaskId { get; set; }

        [Required(ErrorMessage = "Step title is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Step title must be between 1 and 255 characters")]
        public string StepTitle { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Step description cannot exceed 1000 characters")]
        public string? StepDescription { get; set; }

        [Required(ErrorMessage = "Step order is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Step order must be greater than 0")]
        public int StepOrder { get; set; }
    }

    public class UpdateTaskStepDto
    {
        [Required(ErrorMessage = "Task step ID is required")]
        public int TaskStepId { get; set; }

        [Required(ErrorMessage = "User task ID is required")]
        public int UserTaskId { get; set; }

        [Required(ErrorMessage = "Step title is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Step title must be between 1 and 255 characters")]
        public string StepTitle { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Step description cannot exceed 1000 characters")]
        public string? StepDescription { get; set; }

        [Required(ErrorMessage = "Step order is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Step order must be greater than 0")]
        public int StepOrder { get; set; }

        [Required(ErrorMessage = "IsCompleted status is required")]
        public bool IsCompleted { get; set; }
    }

    public class TaskStepResponseDto
    {
        public int TaskStepId { get; set; }

        public int UserTaskId { get; set; }

        public string StepTitle { get; set; } = null!;

        public string? StepDescription { get; set; }

        public int StepOrder { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
