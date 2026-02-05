using System;
using System.Collections.Generic;

namespace TaskManager.API.Models;

public partial class TaskStep
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

    public virtual UserTask UserTask { get; set; } = null!;
}
