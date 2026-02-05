using System;
using System.Collections.Generic;

namespace TaskManager.API.Models;

public partial class TaskRecurrence
{
    public int RecurrenceId { get; set; }

    public string RecurrenceType { get; set; } = null!;

    public int IntervalDays { get; set; }

    public DateOnly? RecurrenceEndDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<TaskNotification> TaskNotifications { get; set; } = new List<TaskNotification>();

    public virtual ICollection<TaskRecurrenceDay> TaskRecurrenceDays { get; set; } = new List<TaskRecurrenceDay>();

    public virtual ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
}
