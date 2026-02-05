using System;
using System.Collections.Generic;

namespace TaskManager.API.Models;

public partial class Organization
{
    public int OrganizationId { get; set; }

    public string OrganizationName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual ICollection<ProgramModel> Programs { get; set; } = new List<ProgramModel>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
