using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class OrgProgramResponseDto
    {
        public int OrgProgramId { get; set; }

        public int OrganizationId { get; set; }

        public string ProgramName { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public OrganizationResponseDto? Organization { get; set; }
    }
}
