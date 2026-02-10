using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Models;

[Table("TaskNotification")]
public partial class TaskNotification
{
    [Key]
    [Column("TaskNotificationID")]
    public int TaskNotificationId { get; set; }

    [Column("RecurrenceID")]
    public int? RecurrenceId { get; set; }

    public int OffsetValue { get; set; }

    [StringLength(10)]
    public string OffsetType { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public bool IsDeleted { get; set; }

    [Precision(3)]
    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    [Precision(3)]
    public DateTime CreatedAt { get; set; }

    [Precision(3)]
    public DateTime UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    [InverseProperty("TaskNotificationCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("DeletedBy")]
    [InverseProperty("TaskNotificationDeletedByNavigations")]
    public virtual User? DeletedByNavigation { get; set; }

    [ForeignKey("RecurrenceId")]
    [InverseProperty("TaskNotifications")]
    public virtual TaskRecurrence? Recurrence { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("TaskNotificationUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }
}
