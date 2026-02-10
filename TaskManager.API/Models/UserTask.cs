using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Models;

[Table("UserTask")]
public partial class UserTask
{
    [Key]
    [Column("UserTaskID")]
    public int UserTaskId { get; set; }

    [StringLength(200)]
    public string TaskName { get; set; } = null!;

    [StringLength(1000)]
    public string? TaskDescription { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public TimeOnly LocalTime { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("RecurrenceID")]
    public int? RecurrenceId { get; set; }

    public bool IsDeleted { get; set; }

    [Precision(3)]
    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    [Precision(3)]
    public DateTime CreatedAt { get; set; }

    [Precision(3)]
    public DateTime UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    [InverseProperty("UserTaskCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("DeletedBy")]
    [InverseProperty("UserTaskDeletedByNavigations")]
    public virtual User? DeletedByNavigation { get; set; }

    [ForeignKey("RecurrenceId")]
    [InverseProperty("UserTasks")]
    public virtual TaskRecurrence? Recurrence { get; set; }

    [InverseProperty("UserTask")]
    public virtual ICollection<TaskStep> TaskSteps { get; set; } = new List<TaskStep>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("UserTaskUpdatedByNavigations")]
    public virtual User UpdatedByNavigation { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserTaskUsers")]
    public virtual User User { get; set; } = null!;
}
