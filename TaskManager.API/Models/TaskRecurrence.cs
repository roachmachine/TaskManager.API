using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Models;

[Table("TaskRecurrence")]
public partial class TaskRecurrence
{
    [Key]
    [Column("RecurrenceID")]
    public int RecurrenceId { get; set; }

    [StringLength(20)]
    public string RecurrenceType { get; set; } = null!;

    public int IntervalDays { get; set; }

    public DateOnly? RecurrenceEndDate { get; set; }

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
    [InverseProperty("TaskRecurrenceCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("DeletedBy")]
    [InverseProperty("TaskRecurrenceDeletedByNavigations")]
    public virtual User? DeletedByNavigation { get; set; }

    [InverseProperty("Recurrence")]
    public virtual ICollection<TaskNotification> TaskNotifications { get; set; } = new List<TaskNotification>();

    [InverseProperty("Recurrence")]
    public virtual ICollection<TaskRecurrenceDay> TaskRecurrenceDays { get; set; } = new List<TaskRecurrenceDay>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("TaskRecurrenceUpdatedByNavigations")]
    public virtual User UpdatedByNavigation { get; set; } = null!;

    [InverseProperty("Recurrence")]
    public virtual ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
}
