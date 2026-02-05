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

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<ProgramModel> Programs { get; set; }

    public virtual DbSet<TaskNotification> TaskNotifications { get; set; }

    public virtual DbSet<TaskRecurrence> TaskRecurrences { get; set; }

    public virtual DbSet<TaskRecurrenceDay> TaskRecurrenceDays { get; set; }

    public virtual DbSet<TaskStep> TaskSteps { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserTask> UserTasks { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is configured in Program.cs through dependency injection
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organization");

            entity.HasIndex(e => e.OrganizationName, "UQ_Organization_OrganizationName").IsUnique();

            entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OrganizationName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<ProgramModel>(entity =>
        {
            entity.HasKey(e => e.ProgramId).HasName("PK__Program__75256038FBC7EF01");

            entity.ToTable("Program");

            entity.HasIndex(e => e.OrganizationId, "IX_Program_OrganizationID").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => new { e.ProgramName, e.OrganizationId }, "UQ_Program_Name_Organization").IsUnique();

            entity.Property(e => e.ProgramId).HasColumnName("ProgramID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");
            entity.Property(e => e.ProgramName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Organization).WithMany(p => p.Programs)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Program_Organization");
        });

        modelBuilder.Entity<TaskNotification>(entity =>
        {
            entity.HasKey(e => e.TaskNotificationId).HasName("PK__TaskNoti__B0B3FAED3A660810");

            entity.ToTable("TaskNotification");

            entity.HasIndex(e => e.RecurrenceId, "IX_TaskNotification_RecurrenceID").HasFilter("([IsEnabled]=(1))");

            entity.Property(e => e.TaskNotificationId).HasColumnName("TaskNotificationID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.OffsetType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.RecurrenceId).HasColumnName("RecurrenceID");

            entity.HasOne(d => d.Recurrence).WithMany(p => p.TaskNotifications)
                .HasForeignKey(d => d.RecurrenceId)
                .HasConstraintName("FK_TaskNotification_TaskRecurrence");
        });

        modelBuilder.Entity<TaskRecurrence>(entity =>
        {
            entity.HasKey(e => e.RecurrenceId).HasName("PK__TaskRecu__9D537B75901395A9");

            entity.ToTable("TaskRecurrence");

            entity.Property(e => e.RecurrenceId).HasColumnName("RecurrenceID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IntervalDays).HasDefaultValue(1);
            entity.Property(e => e.RecurrenceType)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TaskRecurrenceDay>(entity =>
        {
            entity.HasKey(e => e.RecurrenceDayId).HasName("PK__TaskRecu__E96A0D6298BCB2A1");

            entity.HasIndex(e => new { e.RecurrenceId, e.WeekNumber, e.DayOfWeek }, "IX_TaskRecurrenceDays_RecurrenceID");

            entity.HasIndex(e => new { e.RecurrenceId, e.DayOfWeek, e.WeekNumber }, "UQ_TaskRecurrenceDays_Recurrence_Day_Week").IsUnique();

            entity.Property(e => e.RecurrenceDayId).HasColumnName("RecurrenceDayID");
            entity.Property(e => e.RecurrenceId).HasColumnName("RecurrenceID");
            entity.Property(e => e.WeekNumber).HasDefaultValue(1);

            entity.HasOne(d => d.Recurrence).WithMany(p => p.TaskRecurrenceDays)
                .HasForeignKey(d => d.RecurrenceId)
                .HasConstraintName("FK_TaskRecurrenceDays_TaskRecurrence");
        });

        modelBuilder.Entity<TaskStep>(entity =>
        {
            entity.HasKey(e => e.TaskStepId).HasName("PK__TaskStep__88484BF58E5C126A");

            entity.HasIndex(e => new { e.UserTaskId, e.StepOrder }, "IX_TaskSteps_UserTaskID");

            entity.HasIndex(e => new { e.UserTaskId, e.StepOrder }, "UQ_TaskSteps_UserTask_Order").IsUnique();

            entity.Property(e => e.TaskStepId).HasColumnName("TaskStepID");
            entity.Property(e => e.CompletedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StepDescription)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.StepOrder).HasDefaultValue(1);
            entity.Property(e => e.StepTitle)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserTaskId).HasColumnName("UserTaskID");

            entity.HasOne(d => d.UserTask).WithMany(p => p.TaskSteps)
                .HasForeignKey(d => d.UserTaskId)
                .HasConstraintName("FK_TaskSteps_UserTask");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasIndex(e => e.OrganizationId, "IX_User_OrganizationID");

            entity.HasIndex(e => e.ProgramId, "IX_User_ProgramID");

            entity.HasIndex(e => e.UserName, "IX_User_UserName").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => e.UserTypeId, "IX_User_UserTypeID");

            entity.HasIndex(e => e.Email, "UQ_User_Email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");
            entity.Property(e => e.ProgramId).HasColumnName("ProgramID");
            entity.Property(e => e.TimeZoneId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("UTC")
                .HasColumnName("TimeZoneID");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UserTypeId).HasColumnName("UserTypeID");

            entity.HasOne(d => d.Organization).WithMany(p => p.Users)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_User_Organization");

            entity.HasOne(d => d.Program).WithMany(p => p.Users)
                .HasForeignKey(d => d.ProgramId)
                .HasConstraintName("FK_User_Program");

            entity.HasOne(d => d.UserType).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_UserType");
        });

        modelBuilder.Entity<UserTask>(entity =>
        {
            entity.HasKey(e => e.UserTaskId).HasName("PK__UserTask__4EF5963FF0C64715");

            entity.ToTable("UserTask");

            entity.HasIndex(e => e.RecurrenceId, "IX_UserTask_RecurrenceID").HasFilter("([RecurrenceID] IS NOT NULL)");

            entity.HasIndex(e => e.StartDate, "IX_UserTask_StartDate").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => e.UserId, "IX_UserTask_UserID");

            entity.Property(e => e.UserTaskId).HasColumnName("UserTaskID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RecurrenceId).HasColumnName("RecurrenceID");
            entity.Property(e => e.TaskDescription)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.TaskName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Recurrence).WithMany(p => p.UserTasks)
                .HasForeignKey(d => d.RecurrenceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_UserTask_TaskRecurrence");

            entity.HasOne(d => d.User).WithMany(p => p.UserTasks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserTask_User");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.ToTable("UserType");

            entity.HasIndex(e => e.UserType1, "UQ_UserType_UserType").IsUnique();

            entity.Property(e => e.UserTypeId)
                .ValueGeneratedNever()
                .HasColumnName("UserTypeID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserType1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("UserType");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
