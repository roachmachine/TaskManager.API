using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateTaskRecurrenceDayDto
    {
        [Required(ErrorMessage = "Recurrence ID is required")]
        public int RecurrenceId { get; set; }

        [Required(ErrorMessage = "Day of week is required")]
        [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Week number is required")]
        [Range(0, 4, ErrorMessage = "Week number must be between 0 and 4")]
        public int WeekNumber { get; set; }
    }

    public class UpdateTaskRecurrenceDayDto
    {
        [Required(ErrorMessage = "Recurrence day ID is required")]
        public int RecurrenceDayId { get; set; }

        [Required(ErrorMessage = "Recurrence ID is required")]
        public int RecurrenceId { get; set; }

        [Required(ErrorMessage = "Day of week is required")]
        [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Week number is required")]
        [Range(0, 4, ErrorMessage = "Week number must be between 0 and 4")]
        public int WeekNumber { get; set; }
    }

    public class TaskRecurrenceDayResponseDto
    {
        public int RecurrenceDayId { get; set; }

        public int RecurrenceId { get; set; }

        public int DayOfWeek { get; set; }

        public int WeekNumber { get; set; }
    }
}
