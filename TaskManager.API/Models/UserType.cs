using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Models;

[Table("UserType")]
public partial class UserType
{
    [Key]
    [Column("UserTypeID")]
    public int UserTypeId { get; set; }

    [Column("UserType")]
    [StringLength(50)]
    public string UserType1 { get; set; } = null!;

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
    [InverseProperty("UserTypeCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("DeletedBy")]
    [InverseProperty("UserTypeDeletedByNavigations")]
    public virtual User? DeletedByNavigation { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("UserTypeUpdatedByNavigations")]
    public virtual User UpdatedByNavigation { get; set; } = null!;

    [InverseProperty("UserType")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
