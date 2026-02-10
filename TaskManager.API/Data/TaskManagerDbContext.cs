using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Models;

namespace TaskManager.API.Data;

public partial class TaskManagerDbContext : DbContext
{
    public TaskManagerDbContext()
    {
    }

    public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<OrgProgram> OrgPrograms { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<TaskNotification> TaskNotifications { get; set; }

    public virtual DbSet<TaskRecurrence> TaskRecurrences { get; set; }

    public virtual DbSet<TaskRecurrenceDay> TaskRecurrenceDays { get; set; }

    public virtual DbSet<TaskStep> TaskSteps { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserTask> UserTasks { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            optionsBuilder.UseSqlServer("Data Source=MICHAEL-LAPTOP;Initial Catalog=TaskManager;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrgProgram>(entity =>
        {
            entity.ToTable("OrgProgram", tb => tb.HasTrigger("TR_OrgProgram_UpdateAudit"));

            entity.HasIndex(e => e.OrganizationId, "IX_OrgProgram_OrganizationID_Active").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.ProgramName, e.OrganizationId }, "UQ_OrgProgram_Name_Organization_Active")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_OrgProgram_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_OrgProgram_UpdatedAt");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.OrgProgramCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrgProgram_CreatedBy_User");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.OrgProgramDeletedByNavigations).HasConstraintName("FK_OrgProgram_DeletedBy_User");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgPrograms).HasConstraintName("FK_OrgProgram_Organization");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.OrgProgramUpdatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrgProgram_UpdatedBy_User");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organization", tb => tb.HasTrigger("TR_Organization_UpdateAudit"));

            entity.HasIndex(e => e.OrganizationName, "IX_Organization_OrganizationName_Active").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.OrganizationName, "UQ_Organization_OrganizationName_Active")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Organization_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Organization_UpdatedAt");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.OrganizationCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Organization_CreatedBy_User");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.OrganizationDeletedByNavigations).HasConstraintName("FK_Organization_DeletedBy_User");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.OrganizationUpdatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Organization_UpdatedBy_User");
        });

        modelBuilder.Entity<TaskNotification>(entity =>
        {
            entity.HasKey(e => e.TaskNotificationId).HasName("PK__TaskNoti__B0B3FAED6CED25E6");

            entity.ToTable("TaskNotification", tb => tb.HasTrigger("TR_TaskNotification_UpdateAudit"));

            entity.HasIndex(e => e.RecurrenceId, "IX_TaskNotification_RecurrenceID_Enabled").HasFilter("([IsEnabled]=(1) AND [IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TaskNotification_CreatedAt");
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TaskNotification_UpdatedAt");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TaskNotificationCreatedByNavigations).HasConstraintName("FK_TaskNotification_CreatedBy_User");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.TaskNotificationDeletedByNavigations).HasConstraintName("FK_TaskNotification_DeletedBy_User");

            entity.HasOne(d => d.Recurrence).WithMany(p => p.TaskNotifications)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_TaskNotification_TaskRecurrence");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TaskNotificationUpdatedByNavigations).HasConstraintName("FK_TaskNotification_UpdatedBy_User");
        });

        modelBuilder.Entity<TaskRecurrence>(entity =>
        {
            entity.HasKey(e => e.RecurrenceId).HasName("PK__TaskRecu__9D537B75A364963A");

            entity.ToTable("TaskRecurrence", tb => tb.HasTrigger("TR_TaskRecurrence_UpdateAudit"));

            entity.HasIndex(e => e.RecurrenceId, "IX_TaskRecurrence_Active").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.RecurrenceEndDate, "IX_TaskRecurrence_EndDate_Active").HasFilter("([IsDeleted]=(0) AND [RecurrenceEndDate] IS NOT NULL)");

            entity.HasIndex(e => e.RecurrenceType, "IX_TaskRecurrence_Type_Active").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TaskRecurrence_CreatedAt");
            entity.Property(e => e.IntervalDays).HasDefaultValue(1);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TaskRecurrence_UpdatedAt");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TaskRecurrenceCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskRecurrence_CreatedBy_User");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.TaskRecurrenceDeletedByNavigations).HasConstraintName("FK_TaskRecurrence_DeletedBy_User");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TaskRecurrenceUpdatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskRecurrence_UpdatedBy_User");
        });

        modelBuilder.Entity<TaskRecurrenceDay>(entity =>
        {
            entity.HasKey(e => e.RecurrenceDayId).HasName("PK__TaskRecu__E96A0D6223340962");

            entity.ToTable(tb => tb.HasTrigger("TR_TaskRecurrenceDays_UpdateAudit"));

            entity.HasIndex(e => new { e.RecurrenceId, e.WeekNumber, e.DayOfWeek }, "IX_TaskRecurrenceDays_RecurrenceID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.RecurrenceId, e.DayOfWeek, e.WeekNumber }, "UQ_TaskRecurrenceDays_Recurrence_Day_Week_Active")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TaskRecurrenceDays_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TaskRecurrenceDays_UpdatedAt");
            entity.Property(e => e.WeekNumber).HasDefaultValue(1);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TaskRecurrenceDayCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskRecurrenceDays_CreatedBy_User");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.TaskRecurrenceDayDeletedByNavigations).HasConstraintName("FK_TaskRecurrenceDays_DeletedBy_User");

            entity.HasOne(d => d.Recurrence).WithMany(p => p.TaskRecurrenceDays).HasConstraintName("FK_TaskRecurrenceDays_TaskRecurrence");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TaskRecurrenceDayUpdatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskRecurrenceDays_UpdatedBy_User");
        });

        modelBuilder.Entity<TaskStep>(entity =>
        {
            entity.HasKey(e => e.TaskStepId).HasName("PK__TaskStep__88484BF55E7007C3");

            entity.ToTable(tb => tb.HasTrigger("TR_TaskSteps_UpdateAudit"));

            entity.HasIndex(e => e.UserTaskId, "IX_TaskSteps_UserTaskID_Active").HasFilter("([IsCompleted]=(0) AND [IsDeleted]=(0))");

            entity.HasIndex(e => new { e.UserTaskId, e.CompletedDate }, "IX_TaskSteps_UserTaskID_Completed").HasFilter("([IsCompleted]=(1) AND [IsDeleted]=(0))");

            entity.HasIndex(e => new { e.UserTaskId, e.StepOrder }, "IX_TaskSteps_UserTaskID_Order").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.UserTaskId, e.StepOrder }, "UQ_TaskSteps_UserTask_Order_Active")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TaskSteps_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.StepOrder).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TaskSteps_UpdatedAt");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TaskStepCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskSteps_CreatedBy_User");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.TaskStepDeletedByNavigations).HasConstraintName("FK_TaskSteps_DeletedBy_User");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TaskStepUpdatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskSteps_UpdatedBy_User");

            entity.HasOne(d => d.UserTask).WithMany(p => p.TaskSteps).HasConstraintName("FK_TaskSteps_UserTask");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User", tb => tb.HasTrigger("TR_User_UpdateAudit"));

            entity.HasIndex(e => e.Email, "IX_User_Email_Active").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.OrgProgramId, "IX_User_OrgProgramID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.OrgProgramId, "IX_User_OrgProgramID_Active").HasFilter("([IsDeleted]=(0) AND [OrgProgramID] IS NOT NULL)");

            entity.HasIndex(e => e.UserName, "IX_User_UserName_Active").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.UserTypeId, "IX_User_UserTypeID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.Email, "UQ_User_Email_Active")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_User_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TimeZoneId).HasDefaultValue("UTC");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_User_UpdatedAt");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_CreatedBy_User");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.InverseDeletedByNavigation).HasConstraintName("FK_User_DeletedBy_User");

            entity.HasOne(d => d.OrgProgram).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_User_OrgProgram");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InverseUpdatedByNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_UpdatedBy_User");

            entity.HasOne(d => d.UserType).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_UserType");
        });

        modelBuilder.Entity<UserTask>(entity =>
        {
            entity.HasKey(e => e.UserTaskId).HasName("PK__UserTask__4EF5963FADC7FE0B");

            entity.ToTable("UserTask", tb => tb.HasTrigger("TR_UserTask_UpdateAudit"));

            entity.HasIndex(e => e.EndDate, "IX_UserTask_EndDate_Active").HasFilter("([IsDeleted]=(0) AND [EndDate] IS NOT NULL)");

            entity.HasIndex(e => e.RecurrenceId, "IX_UserTask_RecurrenceID").HasFilter("([RecurrenceID] IS NOT NULL)");

            entity.HasIndex(e => e.StartDate, "IX_UserTask_StartDate_Active").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.UserId, "IX_UserTask_UserID_Active").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.UserId, e.StartDate }, "IX_UserTask_UserID_StartDate_Includes").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_UserTask_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_UserTask_UpdatedAt");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.UserTaskCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserTask_CreatedBy_User");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.UserTaskDeletedByNavigations).HasConstraintName("FK_UserTask_DeletedBy_User");

            entity.HasOne(d => d.Recurrence).WithMany(p => p.UserTasks)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_UserTask_TaskRecurrence");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.UserTaskUpdatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserTask_UpdatedBy_User");

            entity.HasOne(d => d.User).WithMany(p => p.UserTaskUsers).HasConstraintName("FK_UserTask_User");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.ToTable("UserType", tb => tb.HasTrigger("TR_UserType_UpdateAudit"));

            entity.HasIndex(e => e.UserType1, "UQ_UserType_UserType_Active")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_UserType_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_UserType_UpdatedAt");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.UserTypeCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserType_CreatedBy_User");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.UserTypeDeletedByNavigations).HasConstraintName("FK_UserType_DeletedBy_User");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.UserTypeUpdatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserType_UpdatedBy_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public override int SaveChanges()
    {
        GenerateRowVersionForInMemoryDb();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GenerateRowVersionForInMemoryDb();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void GenerateRowVersionForInMemoryDb()
    {
        // Only generate RowVersion for in-memory database
        var providerName = Database.ProviderName;
        if (providerName != null && providerName.Contains("InMemory"))
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var rowVersionProperty = entry.Properties
                    .FirstOrDefault(p => p.Metadata.Name == "RowVersion");

                if (rowVersionProperty != null)
                {
                    // Generate a new RowVersion value (8 bytes)
                    var newRowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
                    rowVersionProperty.CurrentValue = newRowVersion;
                }
            }
        }
    }
}
