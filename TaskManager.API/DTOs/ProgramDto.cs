using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateProgramDto
    {
        [Required(ErrorMessage = "Program name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Program name must be between 1 and 100 characters")]
        public string ProgramName { get; set; } = null!;

        [Required(ErrorMessage = "Organization ID is required")]
        public int OrganizationId { get; set; }
    }

    public class UpdateProgramDto
    {
        [Required(ErrorMessage = "Program ID is required")]
        public int ProgramId { get; set; }

        [Required(ErrorMessage = "Program name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Program name must be between 1 and 100 characters")]
        public string ProgramName { get; set; } = null!;

        [Required(ErrorMessage = "Organization ID is required")]
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "IsActive status is required")]
        public bool IsActive { get; set; }
    }

    public class ProgramResponseDto
    {
        public int ProgramId { get; set; }

        public string ProgramName { get; set; } = null!;

        public int OrganizationId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public OrganizationDto? Organization { get; set; }
    }
}
