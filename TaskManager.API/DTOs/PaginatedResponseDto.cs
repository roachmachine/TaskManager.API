using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class PaginatedResponseDto<T>
    {
        [Required]
        public List<T> Data { get; set; } = new();

        [Required]
        public int Total { get; set; }

        [Required]
        public int PageNumber { get; set; }

        [Required]
        public int PageSize { get; set; }
    }
}
