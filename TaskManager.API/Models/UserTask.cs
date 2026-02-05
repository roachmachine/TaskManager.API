using System;
using System.Collections.Generic;

namespace TaskManager.API.Models;

public partial class UserTask
{
    public int UserTaskId { get; set; }

    public string TaskName { get; set; } = null!;

    public string? TaskDescription { get; set; }

    public TimeOnly LocalTime { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int UserId { get; set; }

    public int? RecurrenceId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual TaskRecurrence? Recurrence { get; set; }

    public virtual ICollection<TaskStep> TaskSteps { get; set; } = new List<TaskStep>();

    public virtual User User { get; set; } = null!;
}
