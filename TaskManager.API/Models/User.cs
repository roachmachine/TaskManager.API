using System;
using System.Collections.Generic;

namespace TaskManager.API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int UserTypeId { get; set; }

    public int? OrganizationId { get; set; }

    public int? ProgramId { get; set; }

    public string TimeZoneId { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual Organization? Organization { get; set; }

    public virtual ProgramModel? Program { get; set; }

    public virtual ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();

    public virtual UserType UserType { get; set; } = null!;
}
