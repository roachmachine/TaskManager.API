using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Models;

[Table("OrgProgram")]
public partial class OrgProgram
{
    [Key]
    [Column("OrgProgramID")]
    public int OrgProgramId { get; set; }

    [StringLength(100)]
    public string ProgramName { get; set; } = null!;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Column("OrganizationID")]
    public int OrganizationId { get; set; }

    public bool IsDeleted { get; set; }

    [Precision(3)]
    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    [Precision(3)]
    public DateTime CreatedAt { get; set; }

    [Precision(3)]
    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public int CreatedBy { get; set; }

    public int UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("OrgProgramCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("DeletedBy")]
    [InverseProperty("OrgProgramDeletedByNavigations")]
    public virtual User? DeletedByNavigation { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("OrgPrograms")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("OrgProgramUpdatedByNavigations")]
    public virtual User UpdatedByNavigation { get; set; } = null!;

    [InverseProperty("OrgProgram")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
