using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateProgramDto
    {
        [Required(ErrorMessage = "Program name is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Program name must be between 1 and 255 characters")]
        public string ProgramName { get; set; } = null!;

        [Required(ErrorMessage = "Organization ID is required")]
        public int OrganizationId { get; set; }
    }

    public class UpdateProgramDto
    {
        [Required(ErrorMessage = "Program ID is required")]
        public int ProgramId { get; set; }

        [Required(ErrorMessage = "Program name is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Program name must be between 1 and 255 characters")]
        public string ProgramName { get; set; } = null!;

        [Required(ErrorMessage = "Organization ID is required")]
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "IsDeleted status is required")]
        public bool IsDeleted { get; set; }
    }

    public class ProgramResponseDto
    {
        public int ProgramId { get; set; }

        public int OrganizationId { get; set; }

        public string ProgramName { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public OrganizationResponseDto? Organization { get; set; }
    }
}
