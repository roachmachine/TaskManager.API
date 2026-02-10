using System.ComponentModel.DataAnnotations;
using TaskManager.API.Models;

namespace TaskManager.API.DTOs
{
    public class CreateOrganizationDto
    {
        [Required(ErrorMessage = "Organization name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Organization name must be between 1 and 100 characters")]
        public string OrganizationName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }
    }

    public class UpdateOrganizationDto
    {
        [Required(ErrorMessage = "Organization ID is required")]
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "Organization name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Organization name must be between 1 and 100 characters")]
        public string OrganizationName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "IsDeleted status is required")]
        public bool IsDeleted { get; set; }
    }

    public class OrganizationResponseDto
    {
        public int OrganizationId { get; set; }

        public string OrganizationName { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<OrgProgramResponseDto> OrgPrograms { get; set; } = new List<OrgProgramResponseDto>();
    }
}
