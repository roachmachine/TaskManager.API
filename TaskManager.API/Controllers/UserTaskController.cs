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
    public class UserTaskController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<UserTaskController> _logger;

        public UserTaskController(TaskManagerDbContext context, ILogger<UserTaskController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all user tasks
        /// </summary>
        /// <returns>List of user tasks</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<UserTaskResponseDto>>> GetUserTasks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? userId = null)
        {
            try
            {
                var query = _context.UserTasks.Where(ut => !ut.IsDeleted);

                if (userId.HasValue)
                {
                    query = query.Where(ut => ut.UserId == userId);
                }

                var total = await query.CountAsync();
                var tasks = await query
                    .OrderBy(ut => ut.TaskName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(ut => ut.User)
                    .Include(ut => ut.Recurrence)
                    .Include(ut => ut.TaskSteps)
                    .ToListAsync();

                var taskDtos = tasks.Select(t => new UserTaskResponseDto
                {
                    UserTaskId = t.UserTaskId,
                    TaskName = t.TaskName,
                    TaskDescription = t.TaskDescription,
                    LocalTime = t.LocalTime,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    UserId = t.UserId,
                    RecurrenceId = t.RecurrenceId,
                    IsDeleted = t.IsDeleted,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    User = t.User != null ? new UserResponseDto
                    {
                        UserId = t.User.UserId,
                        UserName = t.User.UserName,
                        Email = t.User.Email,
                        UserTypeId = t.User.UserTypeId,
                        OrgProgramId = t.User.OrgProgramId,
                        TimeZoneId = t.User.TimeZoneId,
                        IsDeleted = t.User.IsDeleted,
                        CreatedAt = t.User.CreatedAt,
                        UpdatedAt = t.User.UpdatedAt
                    } : null,
                    Recurrence = t.Recurrence != null ? new TaskRecurrenceResponseDto
                    {
                        RecurrenceId = t.Recurrence.RecurrenceId,
                        RecurrenceType = t.Recurrence.RecurrenceType,
                        IntervalDays = t.Recurrence.IntervalDays,
                        RecurrenceEndDate = t.Recurrence.RecurrenceEndDate,
                        CreatedAt = t.Recurrence.CreatedAt
                    } : null,
                    TaskSteps = t.TaskSteps.Select(ts => new TaskStepResponseDto
                    {
                        TaskStepId = ts.TaskStepId,
                        UserTaskId = ts.UserTaskId,
                        StepTitle = ts.StepTitle,
                        StepDescription = ts.StepDescription,
                        StepOrder = ts.StepOrder,
                        IsCompleted = ts.IsCompleted,
                        CompletedDate = ts.CompletedDate,
                        CreatedAt = ts.CreatedAt,
                        UpdatedAt = ts.UpdatedAt
                    }).ToList()
                }).ToList();

                var response = new PaginatedResponseDto<UserTaskResponseDto>
                {
                    Data = taskDtos,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user tasks");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user tasks");
            }
        }

        /// <summary>
        /// Get user task by ID
        /// </summary>
        /// <param name="id">User task ID</param>
        /// <returns>User task details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserTaskResponseDto>> GetUserTask(int id)
        {
            try
            {
                var task = await _context.UserTasks
                    .Include(ut => ut.User)
                    .Include(ut => ut.Recurrence)
                    .Include(ut => ut.TaskSteps)
                    .FirstOrDefaultAsync(ut => ut.UserTaskId == id);

                if (task == null)
                {
                    _logger.LogWarning("User task with ID {UserTaskId} not found", id);
                    return NotFound($"User task with ID {id} not found");
                }

                var taskDto = new UserTaskResponseDto
                {
                    UserTaskId = task.UserTaskId,
                    TaskName = task.TaskName,
                    TaskDescription = task.TaskDescription,
                    LocalTime = task.LocalTime,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    UserId = task.UserId,
                    RecurrenceId = task.RecurrenceId,
                    IsDeleted = task.IsDeleted,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    User = task.User != null ? new UserResponseDto
                    {
                        UserId = task.User.UserId,
                        UserName = task.User.UserName,
                        Email = task.User.Email,
                        UserTypeId = task.User.UserTypeId,
                        OrgProgramId = task.User.OrgProgramId,
                        TimeZoneId = task.User.TimeZoneId,
                        IsDeleted = task.User.IsDeleted,
                        CreatedAt = task.User.CreatedAt,
                        UpdatedAt = task.User.UpdatedAt
                    } : null,
                    Recurrence = task.Recurrence != null ? new TaskRecurrenceResponseDto
                    {
                        RecurrenceId = task.Recurrence.RecurrenceId,
                        RecurrenceType = task.Recurrence.RecurrenceType,
                        IntervalDays = task.Recurrence.IntervalDays,
                        RecurrenceEndDate = task.Recurrence.RecurrenceEndDate,
                        CreatedAt = task.Recurrence.CreatedAt
                    } : null,
                    TaskSteps = task.TaskSteps.Select(ts => new TaskStepResponseDto
                    {
                        TaskStepId = ts.TaskStepId,
                        UserTaskId = ts.UserTaskId,
                        StepTitle = ts.StepTitle,
                        StepDescription = ts.StepDescription,
                        StepOrder = ts.StepOrder,
                        IsCompleted = ts.IsCompleted,
                        CompletedDate = ts.CompletedDate,
                        CreatedAt = ts.CreatedAt,
                        UpdatedAt = ts.UpdatedAt
                    }).ToList()
                };

                return Ok(taskDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user task with ID {UserTaskId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user task");
            }
        }

        /// <summary>
        /// Create a new user task
        /// </summary>
        /// <param name="taskDto">User task data</param>
        /// <returns>Created user task</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserTaskResponseDto>> CreateUserTask([FromBody] CreateUserTaskDto taskDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var task = new UserTask
                {
                    TaskName = taskDto.TaskName,
                    TaskDescription = taskDto.TaskDescription,
                    LocalTime = taskDto.LocalTime,
                    StartDate = taskDto.StartDate,
                    EndDate = taskDto.EndDate,
                    UserId = taskDto.UserId,
                    RecurrenceId = taskDto.RecurrenceId,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserTasks.Add(task);
                await _context.SaveChangesAsync();

                // Map entity to response DTO
                var responseDto = new UserTaskResponseDto
                {
                    UserTaskId = task.UserTaskId,
                    TaskName = task.TaskName,
                    TaskDescription = task.TaskDescription,
                    LocalTime = task.LocalTime,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    UserId = task.UserId,
                    RecurrenceId = task.RecurrenceId,
                    IsDeleted = task.IsDeleted,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt
                };

                return CreatedAtAction(nameof(GetUserTask), new { id = task.UserTaskId }, responseDto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating user task");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user task");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user task");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user task");
            }
        }

        /// <summary>
        /// Update an existing user task
        /// </summary>
        /// <param name="id">User task ID</param>
        /// <param name="taskDto">Updated user task data</param>
        /// <returns>Updated user task</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserTaskResponseDto>> UpdateUserTask(int id, [FromBody] UpdateUserTaskDto taskDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != taskDto.UserTaskId)
                {
                    return BadRequest("User task ID mismatch");
                }

                var existingTask = await _context.UserTasks.FindAsync(id);

                if (existingTask == null)
                {
                    _logger.LogWarning("User task with ID {UserTaskId} not found", id);
                    return NotFound($"User task with ID {id} not found");
                }

                existingTask.TaskName = taskDto.TaskName;
                existingTask.TaskDescription = taskDto.TaskDescription;
                existingTask.LocalTime = taskDto.LocalTime;
                existingTask.StartDate = taskDto.StartDate;
                existingTask.EndDate = taskDto.EndDate;
                existingTask.UserId = taskDto.UserId;
                existingTask.RecurrenceId = taskDto.RecurrenceId;
                existingTask.IsDeleted = taskDto.IsDeleted;
                existingTask.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(existingTask);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating user task with ID {UserTaskId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The user task was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating user task");
                return StatusCode(500, "Database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user task");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a user task (soft delete - marks as inactive)
        /// </summary>
        /// <param name="id">User task ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserTask(int id)
        {
            try
            {
                var task = await _context.UserTasks.FindAsync(id);

                if (task == null)
                {
                    _logger.LogWarning("User task with ID {UserTaskId} not found", id);
                    return NotFound($"User task with ID {id} not found");
                }

                task.IsDeleted = true;
                task.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting user task with ID {UserTaskId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user task with ID {UserTaskId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the user task");
            }
        }
    }
}
