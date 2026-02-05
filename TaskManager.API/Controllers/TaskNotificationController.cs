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
    public class TaskNotificationController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<TaskNotificationController> _logger;

        public TaskNotificationController(TaskManagerDbContext context, ILogger<TaskNotificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all task notifications
        /// </summary>
        /// <returns>List of task notifications</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<TaskNotificationResponseDto>>> GetTaskNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? recurrenceId = null)
        {
            try
            {
                var query = _context.TaskNotifications.Where(tn => tn.IsEnabled);

                if (recurrenceId.HasValue)
                {
                    query = query.Where(tn => tn.RecurrenceId == recurrenceId);
                }

                var total = await query.CountAsync();
                var notifications = await query
                    .OrderBy(tn => tn.OffsetValue)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var notificationDtos = notifications.Select(n => new TaskNotificationResponseDto
                {
                    TaskNotificationId = n.TaskNotificationId,
                    RecurrenceId = n.RecurrenceId,
                    OffsetValue = n.OffsetValue,
                    OffsetType = n.OffsetType,
                    IsEnabled = n.IsEnabled,
                    CreatedDate = n.CreatedDate
                }).ToList();

                var response = new PaginatedResponseDto<TaskNotificationResponseDto>
                {
                    Data = notificationDtos,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task notifications");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving task notifications");
            }
        }

        /// <summary>
        /// Get task notification by ID
        /// </summary>
        /// <param name="id">Task notification ID</param>
        /// <returns>Task notification details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskNotificationResponseDto>> GetTaskNotification(int id)
        {
            try
            {
                var notification = await _context.TaskNotifications.FindAsync(id);

                if (notification == null)
                {
                    _logger.LogWarning("Task notification with ID {TaskNotificationId} not found", id);
                    return NotFound($"Task notification with ID {id} not found");
                }

                var notificationDto = new TaskNotificationResponseDto
                {
                    TaskNotificationId = notification.TaskNotificationId,
                    RecurrenceId = notification.RecurrenceId,
                    OffsetValue = notification.OffsetValue,
                    OffsetType = notification.OffsetType,
                    IsEnabled = notification.IsEnabled,
                    CreatedDate = notification.CreatedDate
                };

                return Ok(notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task notification with ID {TaskNotificationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the task notification");
            }
        }

        /// <summary>
        /// Create a new task notification
        /// </summary>
        /// <param name="notificationDto">Task notification data</param>
        /// <returns>Created task notification</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskNotificationResponseDto>> CreateTaskNotification([FromBody] CreateTaskNotificationDto notificationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notification = new TaskNotification
                {
                    RecurrenceId = notificationDto.RecurrenceId,
                    OffsetValue = notificationDto.OffsetValue,
                    OffsetType = notificationDto.OffsetType,
                    IsEnabled = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.TaskNotifications.Add(notification);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTaskNotification), new { id = notification.TaskNotificationId }, notification);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating task notification");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the task notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task notification");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the task notification");
            }
        }

        /// <summary>
        /// Update an existing task notification
        /// </summary>
        /// <param name="id">Task notification ID</param>
        /// <param name="notificationDto">Updated task notification data</param>
        /// <returns>Updated task notification</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskNotificationResponseDto>> UpdateTaskNotification(int id, [FromBody] UpdateTaskNotificationDto notificationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != notificationDto.TaskNotificationId)
                {
                    return BadRequest("Task notification ID mismatch");
                }

                var existingNotification = await _context.TaskNotifications.FindAsync(id);

                if (existingNotification == null)
                {
                    _logger.LogWarning("Task notification with ID {TaskNotificationId} not found", id);
                    return NotFound($"Task notification with ID {id} not found");
                }

                existingNotification.RecurrenceId = notificationDto.RecurrenceId;
                existingNotification.OffsetValue = notificationDto.OffsetValue;
                existingNotification.OffsetType = notificationDto.OffsetType;
                existingNotification.IsEnabled = notificationDto.IsEnabled;

                await _context.SaveChangesAsync();

                return Ok(existingNotification);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating task notification with ID {TaskNotificationId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The task notification was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating task notification");
                return StatusCode(500, "Database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating task notification");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a task notification (soft delete - marks as disabled)
        /// </summary>
        /// <param name="id">Task notification ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTaskNotification(int id)
        {
            try
            {
                var notification = await _context.TaskNotifications.FindAsync(id);

                if (notification == null)
                {
                    _logger.LogWarning("Task notification with ID {TaskNotificationId} not found", id);
                    return NotFound($"Task notification with ID {id} not found");
                }

                notification.IsEnabled = false;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting task notification with ID {TaskNotificationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task notification with ID {TaskNotificationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the task notification");
            }
        }
    }
}
