using System;
using System.Collections.Generic;

namespace TaskManager.API.Models;

public partial class TaskNotification
{
    public int TaskNotificationId { get; set; }

    public int RecurrenceId { get; set; }

    public int OffsetValue { get; set; }

    public string OffsetType { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual TaskRecurrence Recurrence { get; set; } = null!;
}
