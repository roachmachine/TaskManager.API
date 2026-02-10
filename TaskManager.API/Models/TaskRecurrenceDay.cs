using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Models;

public partial class TaskRecurrenceDay
{
    [Key]
    [Column("RecurrenceDayID")]
    public int RecurrenceDayId { get; set; }

    [Column("RecurrenceID")]
    public int RecurrenceId { get; set; }

    public int DayOfWeek { get; set; }

    public int WeekNumber { get; set; }

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
    [InverseProperty("TaskRecurrenceDayCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("DeletedBy")]
    [InverseProperty("TaskRecurrenceDayDeletedByNavigations")]
    public virtual User? DeletedByNavigation { get; set; }

    [ForeignKey("RecurrenceId")]
    [InverseProperty("TaskRecurrenceDays")]
    public virtual TaskRecurrence Recurrence { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("TaskRecurrenceDayUpdatedByNavigations")]
    public virtual User UpdatedByNavigation { get; set; } = null!;
}
