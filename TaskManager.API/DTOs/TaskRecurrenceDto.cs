using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateTaskRecurrenceDto
    {
        [Required(ErrorMessage = "Recurrence type is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Recurrence type must be between 1 and 50 characters")]
        public string RecurrenceType { get; set; } = null!;

        [Required(ErrorMessage = "Interval days is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Interval days must be greater than 0")]
        public int IntervalDays { get; set; }

        public DateOnly? RecurrenceEndDate { get; set; }
    }

    public class UpdateTaskRecurrenceDto
    {
        [Required(ErrorMessage = "Recurrence ID is required")]
        public int RecurrenceId { get; set; }

        [Required(ErrorMessage = "Recurrence type is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Recurrence type must be between 1 and 50 characters")]
        public string RecurrenceType { get; set; } = null!;

        [Required(ErrorMessage = "Interval days is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Interval days must be greater than 0")]
        public int IntervalDays { get; set; }

        public DateOnly? RecurrenceEndDate { get; set; }
    }

    public class TaskRecurrenceResponseDto
    {
        public int RecurrenceId { get; set; }

        public string RecurrenceType { get; set; } = null!;

        public int IntervalDays { get; set; }

        public DateOnly? RecurrenceEndDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public ICollection<TaskRecurrenceDayResponseDto> TaskRecurrenceDays { get; set; } = new List<TaskRecurrenceDayResponseDto>();
    }
}
