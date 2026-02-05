using System;
using System.Collections.Generic;

namespace TaskManager.API.Models;

public partial class UserType
{
    public int UserTypeId { get; set; }

    public string UserType1 { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
