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
    public class UserController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(TaskManagerDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<UserResponseDto>>> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? organizationId = null)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                if (organizationId.HasValue)
                {
                    query = query.Where(u => u.OrganizationId == organizationId);
                }

                var total = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(u => u.Organization)
                    .Include(u => u.UserType)
                    .ToListAsync();

                var userDtos = users.Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                    UserTypeId = u.UserTypeId,
                    OrganizationId = u.OrganizationId,
                    ProgramId = u.ProgramId,
                    TimeZoneId = u.TimeZoneId,
                    IsActive = u.IsActive,
                    CreateDate = u.CreateDate,
                    UpdateDate = u.UpdateDate,
                    Organization = u.Organization != null ? new OrganizationDto
                    {
                        OrganizationId = u.Organization.OrganizationId,
                        OrganizationName = u.Organization.OrganizationName,
                        IsDeleted = u.Organization.IsDeleted,
                        CreateDate = u.Organization.CreateDate,
                        UpdateDate = u.Organization.UpdateDate
                    } : null,
                    UserType = u.UserType != null ? new UserTypeResponseDto
                    {
                        UserTypeId = u.UserType.UserTypeId,
                        UserType = u.UserType.UserType1,
                        CreateDate = u.UserType.CreateDate
                    } : null
                }).ToList();

                var response = new PaginatedResponseDto<UserResponseDto>
                {
                    Data = userDtos,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving users");
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Organization)
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound($"User with ID {id} not found");
                }

                var userDto = new UserResponseDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    UserTypeId = user.UserTypeId,
                    OrganizationId = user.OrganizationId,
                    ProgramId = user.ProgramId,
                    TimeZoneId = user.TimeZoneId,
                    IsActive = user.IsActive,
                    CreateDate = user.CreateDate,
                    UpdateDate = user.UpdateDate,
                    Organization = user.Organization != null ? new OrganizationDto
                    {
                        OrganizationId = user.Organization.OrganizationId,
                        OrganizationName = user.Organization.OrganizationName,
                        IsDeleted = user.Organization.IsDeleted,
                        CreateDate = user.Organization.CreateDate,
                        UpdateDate = user.Organization.UpdateDate
                    } : null,
                    UserType = user.UserType != null ? new UserTypeResponseDto
                    {
                        UserTypeId = user.UserType.UserTypeId,
                        UserType = user.UserType.UserType1,
                        CreateDate = user.UserType.CreateDate
                    } : null
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user");
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="userDto">User data</param>
        /// <returns>Created user</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = new User
                {
                    UserName = userDto.UserName,
                    Email = userDto.Email,
                    UserTypeId = userDto.UserTypeId,
                    OrganizationId = userDto.OrganizationId,
                    ProgramId = userDto.ProgramId,
                    TimeZoneId = userDto.TimeZoneId,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating user");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user");
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="userDto">Updated user data</param>
        /// <returns>Updated user</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, [FromBody] UpdateUserDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != userDto.UserId)
                {
                    return BadRequest("User ID mismatch");
                }

                var existingUser = await _context.Users.FindAsync(id);

                if (existingUser == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound($"User with ID {id} not found");
                }

                existingUser.UserName = userDto.UserName;
                existingUser.Email = userDto.Email;
                existingUser.UserTypeId = userDto.UserTypeId;
                existingUser.OrganizationId = userDto.OrganizationId;
                existingUser.ProgramId = userDto.ProgramId;
                existingUser.TimeZoneId = userDto.TimeZoneId;
                existingUser.IsActive = userDto.IsActive;
                existingUser.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(existingUser);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The user was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating user");
                return StatusCode(500, "Database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a user (soft delete - marks as inactive)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound($"User with ID {id} not found");
                }

                user.IsActive = false;
                user.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the user");
            }
        }
    }
}
