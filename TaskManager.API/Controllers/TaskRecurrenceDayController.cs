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
    public class TaskRecurrenceDayController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<TaskRecurrenceDayController> _logger;

        public TaskRecurrenceDayController(TaskManagerDbContext context, ILogger<TaskRecurrenceDayController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all task recurrence days
        /// </summary>
        /// <returns>List of task recurrence days</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<TaskRecurrenceDayResponseDto>>> GetTaskRecurrenceDays(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? recurrenceId = null)
        {
            try
            {
                var query = _context.TaskRecurrenceDays.AsQueryable();

                if (recurrenceId.HasValue)
                {
                    query = query.Where(trd => trd.RecurrenceId == recurrenceId);
                }

                var total = await query.CountAsync();
                var days = await query
                    .OrderBy(trd => trd.RecurrenceDayId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dayDtos = days.Select(d => new TaskRecurrenceDayResponseDto
                {
                    RecurrenceDayId = d.RecurrenceDayId,
                    RecurrenceId = d.RecurrenceId,
                    DayOfWeek = d.DayOfWeek,
                    WeekNumber = d.WeekNumber
                }).ToList();

                var response = new PaginatedResponseDto<TaskRecurrenceDayResponseDto>
                {
                    Data = dayDtos,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task recurrence days");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving task recurrence days");
            }
        }

        /// <summary>
        /// Get task recurrence day by ID
        /// </summary>
        /// <param name="id">Task recurrence day ID</param>
        /// <returns>Task recurrence day details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskRecurrenceDayResponseDto>> GetTaskRecurrenceDay(int id)
        {
            try
            {
                var day = await _context.TaskRecurrenceDays.FindAsync(id);

                if (day == null)
                {
                    _logger.LogWarning("Task recurrence day with ID {RecurrenceDayId} not found", id);
                    return NotFound($"Task recurrence day with ID {id} not found");
                }

                var dayDto = new TaskRecurrenceDayResponseDto
                {
                    RecurrenceDayId = day.RecurrenceDayId,
                    RecurrenceId = day.RecurrenceId,
                    DayOfWeek = day.DayOfWeek,
                    WeekNumber = day.WeekNumber
                };

                return Ok(dayDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task recurrence day with ID {RecurrenceDayId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the task recurrence day");
            }
        }

        /// <summary>
        /// Create a new task recurrence day
        /// </summary>
        /// <param name="dayDto">Task recurrence day data</param>
        /// <returns>Created task recurrence day</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskRecurrenceDayResponseDto>> CreateTaskRecurrenceDay([FromBody] CreateTaskRecurrenceDayDto dayDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate DayOfWeek range
                if (dayDto.DayOfWeek < 0 || dayDto.DayOfWeek > 6)
                {
                    return BadRequest("Day of week must be between 0 (Sunday) and 6 (Saturday)");
                }

                var day = new TaskRecurrenceDay
                {
                    RecurrenceId = dayDto.RecurrenceId,
                    DayOfWeek = dayDto.DayOfWeek,
                    WeekNumber = dayDto.WeekNumber
                };

                _context.TaskRecurrenceDays.Add(day);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTaskRecurrenceDay), new { id = day.RecurrenceDayId }, day);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating task recurrence day");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the task recurrence day");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task recurrence day");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the task recurrence day");
            }
        }

        /// <summary>
        /// Update an existing task recurrence day
        /// </summary>
        /// <param name="id">Task recurrence day ID</param>
        /// <param name="dayDto">Updated task recurrence day data</param>
        /// <returns>Updated task recurrence day</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskRecurrenceDayResponseDto>> UpdateTaskRecurrenceDay(int id, [FromBody] UpdateTaskRecurrenceDayDto dayDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != dayDto.RecurrenceDayId)
                {
                    return BadRequest("Task recurrence day ID mismatch");
                }

                // Validate DayOfWeek range
                if (dayDto.DayOfWeek < 0 || dayDto.DayOfWeek > 6)
                {
                    return BadRequest("Day of week must be between 0 (Sunday) and 6 (Saturday)");
                }

                var existingDay = await _context.TaskRecurrenceDays.FindAsync(id);

                if (existingDay == null)
                {
                    _logger.LogWarning("Task recurrence day with ID {RecurrenceDayId} not found", id);
                    return NotFound($"Task recurrence day with ID {id} not found");
                }

                existingDay.RecurrenceId = dayDto.RecurrenceId;
                existingDay.DayOfWeek = dayDto.DayOfWeek;
                existingDay.WeekNumber = dayDto.WeekNumber;

                await _context.SaveChangesAsync();

                return Ok(existingDay);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating task recurrence day with ID {RecurrenceDayId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The task recurrence day was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating task recurrence day");
                return StatusCode(500, "Database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating task recurrence day");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a task recurrence day
        /// </summary>
        /// <param name="id">Task recurrence day ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTaskRecurrenceDay(int id)
        {
            try
            {
                var day = await _context.TaskRecurrenceDays.FindAsync(id);

                if (day == null)
                {
                    _logger.LogWarning("Task recurrence day with ID {RecurrenceDayId} not found", id);
                    return NotFound($"Task recurrence day with ID {id} not found");
                }

                _context.TaskRecurrenceDays.Remove(day);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting task recurrence day with ID {RecurrenceDayId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task recurrence day with ID {RecurrenceDayId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the task recurrence day");
            }
        }
    }
}
