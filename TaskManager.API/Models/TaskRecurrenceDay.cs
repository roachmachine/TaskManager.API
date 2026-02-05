using System;
using System.Collections.Generic;

namespace TaskManager.API.Models;

public partial class TaskRecurrenceDay
{
    public int RecurrenceDayId { get; set; }

    public int RecurrenceId { get; set; }

    public int DayOfWeek { get; set; }

    public int WeekNumber { get; set; }

    public virtual TaskRecurrence Recurrence { get; set; } = null!;
}
