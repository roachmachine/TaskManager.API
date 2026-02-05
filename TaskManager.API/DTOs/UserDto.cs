using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 255 characters")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Email must be between 1 and 255 characters")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "User type ID is required")]
        public int UserTypeId { get; set; }

        public int? OrganizationId { get; set; }

        public int? ProgramId { get; set; }

        [Required(ErrorMessage = "Time zone ID is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Time zone ID must be between 1 and 100 characters")]
        public string TimeZoneId { get; set; } = null!;
    }

    public class UpdateUserDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 255 characters")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Email must be between 1 and 255 characters")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "User type ID is required")]
        public int UserTypeId { get; set; }

        public int? OrganizationId { get; set; }

        public int? ProgramId { get; set; }

        [Required(ErrorMessage = "Time zone ID is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Time zone ID must be between 1 and 100 characters")]
        public string TimeZoneId { get; set; } = null!;

        [Required(ErrorMessage = "IsActive status is required")]
        public bool IsActive { get; set; }
    }

    public class UserResponseDto
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int UserTypeId { get; set; }

        public int? OrganizationId { get; set; }

        public int? ProgramId { get; set; }

        public string TimeZoneId { get; set; } = null!;

        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public OrganizationDto? Organization { get; set; }

        public UserTypeResponseDto? UserType { get; set; }
    }
}
