using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateTaskNotificationDto
    {
        [Required(ErrorMessage = "Recurrence ID is required")]
        public int RecurrenceId { get; set; }

        [Required(ErrorMessage = "Offset value is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Offset value must be 0 or greater")]
        public int OffsetValue { get; set; }

        [Required(ErrorMessage = "Offset type is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Offset type must be between 1 and 50 characters")]
        public string OffsetType { get; set; } = null!;
    }

    public class UpdateTaskNotificationDto
    {
        [Required(ErrorMessage = "Task notification ID is required")]
        public int TaskNotificationId { get; set; }

        [Required(ErrorMessage = "Recurrence ID is required")]
        public int RecurrenceId { get; set; }

        [Required(ErrorMessage = "Offset value is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Offset value must be 0 or greater")]
        public int OffsetValue { get; set; }

        [Required(ErrorMessage = "Offset type is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Offset type must be between 1 and 50 characters")]
        public string OffsetType { get; set; } = null!;

        [Required(ErrorMessage = "IsEnabled status is required")]
        public bool IsEnabled { get; set; }
    }

    public class TaskNotificationResponseDto
    {
        public int TaskNotificationId { get; set; }

        public int RecurrenceId { get; set; }

        public int OffsetValue { get; set; }

        public string OffsetType { get; set; } = null!;

        public bool IsEnabled { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
