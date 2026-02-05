using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateOrganizationDto
    {
        [Required(ErrorMessage = "Organization name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Organization name must be between 1 and 100 characters")]
        public string OrganizationName { get; set; } = null!;
    }

    public class UpdateOrganizationDto
    {
        [Required(ErrorMessage = "Organization ID is required")]
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "Organization name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Organization name must be between 1 and 100 characters")]
        public string OrganizationName { get; set; } = null!;

        [Required(ErrorMessage = "IsActive status is required")]
        public bool IsActive { get; set; }
    }

    public class OrganizationDto
    {
        public int OrganizationId { get; set; }

        public string OrganizationName { get; set; } = null!;

        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
