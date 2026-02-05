using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.DTOs;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserTypeController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<UserTypeController> _logger;

        public UserTypeController(TaskManagerDbContext context, ILogger<UserTypeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all user types
        /// </summary>
        /// <returns>List of user types</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<UserTypeResponseDto>>> GetUserTypes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var total = await _context.UserTypes.CountAsync();
                var userTypes = await _context.UserTypes
                    .OrderBy(ut => ut.UserType1)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userTypeDtos = userTypes.Select(ut => new UserTypeResponseDto
                {
                    UserTypeId = ut.UserTypeId,
                    UserType = ut.UserType1,
                    CreateDate = ut.CreateDate
                }).ToList();

                var response = new PaginatedResponseDto<UserTypeResponseDto>
                {
                    Data = userTypeDtos,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user types");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user types");
            }
        }

        /// <summary>
        /// Get user type by ID
        /// </summary>
        /// <param name="id">User type ID</param>
        /// <returns>User type details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserTypeResponseDto>> GetUserType(int id)
        {
            try
            {
                var userType = await _context.UserTypes.FindAsync(id);

                if (userType == null)
                {
                    _logger.LogWarning("User type with ID {UserTypeId} not found", id);
                    return NotFound($"User type with ID {id} not found");
                }

                var userTypeDto = new UserTypeResponseDto
                {
                    UserTypeId = userType.UserTypeId,
                    UserType = userType.UserType1,
                    CreateDate = userType.CreateDate
                };

                return Ok(userTypeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user type with ID {UserTypeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user type");
            }
        }

        /// <summary>
        /// Create a new user type
        /// </summary>
        /// <param name="userTypeDto">User type data</param>
        /// <returns>Created user type</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserTypeResponseDto>> CreateUserType([FromBody] CreateUserTypeDto userTypeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userType = new UserType
                {
                    UserType1 = userTypeDto.UserType,
                    CreateDate = DateTime.UtcNow
                };

                _context.UserTypes.Add(userType);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserType), new { id = userType.UserTypeId }, userType);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating user type");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user type");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user type");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user type");
            }
        }

        /// <summary>
        /// Update an existing user type
        /// </summary>
        /// <param name="id">User type ID</param>
        /// <param name="userTypeDto">Updated user type data</param>
        /// <returns>Updated user type</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserTypeResponseDto>> UpdateUserType(int id, [FromBody] UpdateUserTypeDto userTypeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != userTypeDto.UserTypeId)
                {
                    return BadRequest("User type ID mismatch");
                }

                var existingUserType = await _context.UserTypes.FindAsync(id);

                if (existingUserType == null)
                {
                    _logger.LogWarning("User type with ID {UserTypeId} not found", id);
                    return NotFound($"User type with ID {id} not found");
                }

                existingUserType.UserType1 = userTypeDto.UserType;

                await _context.SaveChangesAsync();

                return Ok(existingUserType);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating user type with ID {UserTypeId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The user type was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating user type");
                return StatusCode(500, "Database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user type");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a user type
        /// </summary>
        /// <param name="id">User type ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserType(int id)
        {
            try
            {
                var userType = await _context.UserTypes.FindAsync(id);

                if (userType == null)
                {
                    _logger.LogWarning("User type with ID {UserTypeId} not found", id);
                    return NotFound($"User type with ID {id} not found");
                }

                _context.UserTypes.Remove(userType);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting user type with ID {UserTypeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user type with ID {UserTypeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the user type");
            }
        }
    }
}
