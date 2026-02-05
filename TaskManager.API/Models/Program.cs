using System;
using System.Collections.Generic;

namespace TaskManager.API.Models;

public partial class ProgramModel
{
    public int ProgramId { get; set; }

    public string ProgramName { get; set; } = null!;

    public int OrganizationId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
