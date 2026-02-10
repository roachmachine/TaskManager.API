using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Models;

[Table("User")]
public partial class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("UserTypeID")]
    public int UserTypeId { get; set; }

    [Column("OrgProgramID")]
    public int? OrgProgramId { get; set; }

    [Column("TimeZoneID")]
    [StringLength(50)]
    public string TimeZoneId { get; set; } = null!;

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
    [InverseProperty("InverseCreatedByNavigation")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("DeletedBy")]
    [InverseProperty("InverseDeletedByNavigation")]
    public virtual User? DeletedByNavigation { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<User> InverseDeletedByNavigation { get; set; } = new List<User>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; } = new List<User>();

    [ForeignKey("OrgProgramId")]
    [InverseProperty("Users")]
    public virtual OrgProgram? OrgProgram { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<OrgProgram> OrgProgramCreatedByNavigations { get; set; } = new List<OrgProgram>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<OrgProgram> OrgProgramDeletedByNavigations { get; set; } = new List<OrgProgram>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<OrgProgram> OrgProgramUpdatedByNavigations { get; set; } = new List<OrgProgram>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Organization> OrganizationCreatedByNavigations { get; set; } = new List<Organization>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<Organization> OrganizationDeletedByNavigations { get; set; } = new List<Organization>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Organization> OrganizationUpdatedByNavigations { get; set; } = new List<Organization>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TaskNotification> TaskNotificationCreatedByNavigations { get; set; } = new List<TaskNotification>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<TaskNotification> TaskNotificationDeletedByNavigations { get; set; } = new List<TaskNotification>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<TaskNotification> TaskNotificationUpdatedByNavigations { get; set; } = new List<TaskNotification>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TaskRecurrence> TaskRecurrenceCreatedByNavigations { get; set; } = new List<TaskRecurrence>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TaskRecurrenceDay> TaskRecurrenceDayCreatedByNavigations { get; set; } = new List<TaskRecurrenceDay>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<TaskRecurrenceDay> TaskRecurrenceDayDeletedByNavigations { get; set; } = new List<TaskRecurrenceDay>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<TaskRecurrenceDay> TaskRecurrenceDayUpdatedByNavigations { get; set; } = new List<TaskRecurrenceDay>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<TaskRecurrence> TaskRecurrenceDeletedByNavigations { get; set; } = new List<TaskRecurrence>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<TaskRecurrence> TaskRecurrenceUpdatedByNavigations { get; set; } = new List<TaskRecurrence>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TaskStep> TaskStepCreatedByNavigations { get; set; } = new List<TaskStep>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<TaskStep> TaskStepDeletedByNavigations { get; set; } = new List<TaskStep>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<TaskStep> TaskStepUpdatedByNavigations { get; set; } = new List<TaskStep>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("InverseUpdatedByNavigation")]
    public virtual User UpdatedByNavigation { get; set; } = null!;

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<UserTask> UserTaskCreatedByNavigations { get; set; } = new List<UserTask>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<UserTask> UserTaskDeletedByNavigations { get; set; } = new List<UserTask>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<UserTask> UserTaskUpdatedByNavigations { get; set; } = new List<UserTask>();

    [InverseProperty("User")]
    public virtual ICollection<UserTask> UserTaskUsers { get; set; } = new List<UserTask>();

    [ForeignKey("UserTypeId")]
    [InverseProperty("Users")]
    public virtual UserType UserType { get; set; } = null!;

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<UserType> UserTypeCreatedByNavigations { get; set; } = new List<UserType>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<UserType> UserTypeDeletedByNavigations { get; set; } = new List<UserType>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<UserType> UserTypeUpdatedByNavigations { get; set; } = new List<UserType>();
}
