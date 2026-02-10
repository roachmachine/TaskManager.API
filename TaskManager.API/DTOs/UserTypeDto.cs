using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class CreateUserTypeDto
    {
        [Required(ErrorMessage = "User type name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "User type name must be between 1 and 100 characters")]
        public string UserType { get; set; } = null!;
    }

    public class UpdateUserTypeDto
    {
        [Required(ErrorMessage = "User type ID is required")]
        public int UserTypeId { get; set; }

        [Required(ErrorMessage = "User type name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "User type name must be between 1 and 100 characters")]
        public string UserType { get; set; } = null!;
    }

    public class UserTypeResponseDto
    {
        public int UserTypeId { get; set; }

        public string UserType { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
