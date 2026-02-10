using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Models;

[Table("Organization")]
public partial class Organization
{
    [Key]
    [Column("OrganizationID")]
    public int OrganizationId { get; set; }

    [StringLength(100)]
    public string OrganizationName { get; set; } = null!;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

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
    [InverseProperty("OrganizationCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("DeletedBy")]
    [InverseProperty("OrganizationDeletedByNavigations")]
    public virtual User? DeletedByNavigation { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<OrgProgram> OrgPrograms { get; set; } = new List<OrgProgram>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("OrganizationUpdatedByNavigations")]
    public virtual User UpdatedByNavigation { get; set; } = null!;
}
