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
            [FromQuery] int? OrgProgramId = null)
        {
            try
            {
                var query = _context.Users.Where(u => !u.IsDeleted);

                if (OrgProgramId.HasValue)
                {
                    query = query.Where(u => u.OrgProgramId == OrgProgramId.Value);
                }

                var total = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(u => u.OrgProgram)
                        .ThenInclude(op => op!.Organization)
                    .Include(u => u.UserType)
                    .ToListAsync();

                var userDtos = users.Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                    UserTypeId = u.UserTypeId,
                    OrgProgramId = u.OrgProgramId,
                    TimeZoneId = u.TimeZoneId,
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    OrgProgram = u.OrgProgram != null ? new OrgProgramResponseDto
                    {
                        OrgProgramId = u.OrgProgram.OrgProgramId,
                        OrganizationId = u.OrgProgram.OrganizationId,
                        ProgramName = u.OrgProgram.ProgramName,
                        IsDeleted = u.OrgProgram.IsDeleted,
                        CreatedAt = u.OrgProgram.CreatedAt,
                        UpdatedAt = u.OrgProgram.UpdatedAt
                    } : null,
                    UserType = u.UserType != null ? new UserTypeResponseDto
                    {
                        UserTypeId = u.UserType.UserTypeId,
                        UserType = u.UserType.UserType1,
                        CreatedAt = u.UserType.CreatedAt
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
                    .Include(u => u.OrgProgram)
                        .ThenInclude(op => op!.Organization)
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);

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
                    OrgProgramId = user.OrgProgramId,
                    TimeZoneId = user.TimeZoneId,
                    IsDeleted = user.IsDeleted,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    OrgProgram = user.OrgProgram != null ? new OrgProgramResponseDto
                    {
                        OrgProgramId = user.OrgProgram.OrgProgramId,
                        OrganizationId = user.OrgProgram.OrganizationId,
                        ProgramName = user.OrgProgram.ProgramName,
                        IsDeleted = user.OrgProgram.IsDeleted,
                        CreatedAt = user.OrgProgram.CreatedAt,
                        UpdatedAt = user.OrgProgram.UpdatedAt
                    } : null,
                    UserType = user.UserType != null ? new UserTypeResponseDto
                    {
                        UserTypeId = user.UserType.UserTypeId,
                        UserType = user.UserType.UserType1,
                        CreatedAt = user.UserType.CreatedAt
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
                    OrgProgramId = userDto.OrgProgramId,
                    TimeZoneId = userDto.TimeZoneId,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = 1,
                    UpdatedBy = 1
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var responseDto = new UserResponseDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    UserTypeId = user.UserTypeId,
                    OrgProgramId = user.OrgProgramId,
                    TimeZoneId = user.TimeZoneId,
                    IsDeleted = user.IsDeleted,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, responseDto);
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

                var existingUser = await _context.Users
                    .Include(u => u.OrgProgram)
                        .ThenInclude(op => op!.Organization)
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (existingUser == null || existingUser.IsDeleted)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound($"User with ID {id} not found");
                }

                existingUser.UserName = userDto.UserName;
                existingUser.Email = userDto.Email;
                existingUser.UserTypeId = userDto.UserTypeId;
                existingUser.OrgProgramId = userDto.OrgProgramId;
                existingUser.TimeZoneId = userDto.TimeZoneId;
                existingUser.IsDeleted = userDto.IsDeleted;
                existingUser.UpdatedAt = DateTime.UtcNow;
                existingUser.UpdatedBy = 1;

                await _context.SaveChangesAsync();

                var responseDto = new UserResponseDto
                {
                    UserId = existingUser.UserId,
                    UserName = existingUser.UserName,
                    Email = existingUser.Email,
                    UserTypeId = existingUser.UserTypeId,
                    OrgProgramId = existingUser.OrgProgramId,
                    TimeZoneId = existingUser.TimeZoneId,
                    IsDeleted = existingUser.IsDeleted,
                    CreatedAt = existingUser.CreatedAt,
                    UpdatedAt = existingUser.UpdatedAt,
                    OrgProgram = existingUser.OrgProgram != null ? new OrgProgramResponseDto
                    {
                        OrgProgramId = existingUser.OrgProgram.OrgProgramId,
                        OrganizationId = existingUser.OrgProgram.OrganizationId,
                        ProgramName = existingUser.OrgProgram.ProgramName,
                        IsDeleted = existingUser.OrgProgram.IsDeleted,
                        CreatedAt = existingUser.OrgProgram.CreatedAt,
                        UpdatedAt = existingUser.OrgProgram.UpdatedAt
                    } : null,
                    UserType = existingUser.UserType != null ? new UserTypeResponseDto
                    {
                        UserTypeId = existingUser.UserType.UserTypeId,
                        UserType = existingUser.UserType.UserType1,
                        CreatedAt = existingUser.UserType.CreatedAt
                    } : null
                };

                return Ok(responseDto);
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
        /// Delete a user (soft delete - marks as deleted)
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

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound($"User with ID {id} not found");
                }

                user.IsDeleted = true;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = 1;

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
