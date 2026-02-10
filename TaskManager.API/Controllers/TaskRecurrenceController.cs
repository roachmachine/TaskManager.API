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
    public class TaskRecurrenceController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<TaskRecurrenceController> _logger;

        public TaskRecurrenceController(TaskManagerDbContext context, ILogger<TaskRecurrenceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all task recurrences
        /// </summary>
        /// <returns>List of task recurrences</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<TaskRecurrenceResponseDto>>> GetTaskRecurrences(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var total = await _context.TaskRecurrences.CountAsync();
                var recurrences = await _context.TaskRecurrences
                    .OrderBy(tr => tr.RecurrenceId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(tr => tr.TaskRecurrenceDays)
                    .ToListAsync();

                var recurrenceDtos = recurrences.Select(tr => new TaskRecurrenceResponseDto
                {
                    RecurrenceId = tr.RecurrenceId,
                    RecurrenceType = tr.RecurrenceType,
                    IntervalDays = tr.IntervalDays,
                    RecurrenceEndDate = tr.RecurrenceEndDate,
                    CreatedAt = tr.CreatedAt,
                    TaskRecurrenceDays = tr.TaskRecurrenceDays.Select(trd => new TaskRecurrenceDayResponseDto
                    {
                        RecurrenceDayId = trd.RecurrenceDayId,
                        RecurrenceId = trd.RecurrenceId,
                        DayOfWeek = trd.DayOfWeek,
                        WeekNumber = trd.WeekNumber
                    }).ToList()
                }).ToList();

                var response = new PaginatedResponseDto<TaskRecurrenceResponseDto>
                {
                    Data = recurrenceDtos,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task recurrences");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving task recurrences");
            }
        }

        /// <summary>
        /// Get task recurrence by ID
        /// </summary>
        /// <param name="id">Task recurrence ID</param>
        /// <returns>Task recurrence details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskRecurrenceResponseDto>> GetTaskRecurrence(int id)
        {
            try
            {
                var recurrence = await _context.TaskRecurrences
                    .Include(tr => tr.TaskRecurrenceDays)
                    .FirstOrDefaultAsync(tr => tr.RecurrenceId == id);

                if (recurrence == null)
                {
                    _logger.LogWarning("Task recurrence with ID {RecurrenceId} not found", id);
                    return NotFound($"Task recurrence with ID {id} not found");
                }

                var recurrenceDto = new TaskRecurrenceResponseDto
                {
                    RecurrenceId = recurrence.RecurrenceId,
                    RecurrenceType = recurrence.RecurrenceType,
                    IntervalDays = recurrence.IntervalDays,
                    RecurrenceEndDate = recurrence.RecurrenceEndDate,
                    CreatedAt = recurrence.CreatedAt,
                    TaskRecurrenceDays = recurrence.TaskRecurrenceDays.Select(trd => new TaskRecurrenceDayResponseDto
                    {
                        RecurrenceDayId = trd.RecurrenceDayId,
                        RecurrenceId = trd.RecurrenceId,
                        DayOfWeek = trd.DayOfWeek,
                        WeekNumber = trd.WeekNumber
                    }).ToList()
                };

                return Ok(recurrenceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task recurrence with ID {RecurrenceId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the task recurrence");
            }
        }

        /// <summary>
        /// Create a new task recurrence
        /// </summary>
        /// <param name="recurrenceDto">Task recurrence data</param>
        /// <returns>Created task recurrence</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskRecurrenceResponseDto>> CreateTaskRecurrence([FromBody] CreateTaskRecurrenceDto recurrenceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var recurrence = new TaskRecurrence
                {
                    RecurrenceType = recurrenceDto.RecurrenceType,
                    IntervalDays = recurrenceDto.IntervalDays,
                    RecurrenceEndDate = recurrenceDto.RecurrenceEndDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = 1,
                    UpdatedBy = 1
                };

                _context.TaskRecurrences.Add(recurrence);
                await _context.SaveChangesAsync();

                var responseDto = new TaskRecurrenceResponseDto
                {
                    RecurrenceId = recurrence.RecurrenceId,
                    RecurrenceType = recurrence.RecurrenceType,
                    IntervalDays = recurrence.IntervalDays,
                    RecurrenceEndDate = recurrence.RecurrenceEndDate,
                    CreatedAt = recurrence.CreatedAt,
                    TaskRecurrenceDays = new List<TaskRecurrenceDayResponseDto>()
                };

                return CreatedAtAction(nameof(GetTaskRecurrence), new { id = recurrence.RecurrenceId }, responseDto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating task recurrence");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the task recurrence");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task recurrence");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the task recurrence");
            }
        }

        /// <summary>
        /// Update an existing task recurrence
        /// </summary>
        /// <param name="id">Task recurrence ID</param>
        /// <param name="recurrenceDto">Updated task recurrence data</param>
        /// <returns>Updated task recurrence</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskRecurrenceResponseDto>> UpdateTaskRecurrence(int id, [FromBody] UpdateTaskRecurrenceDto recurrenceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != recurrenceDto.RecurrenceId)
                {
                    return BadRequest("Task recurrence ID mismatch");
                }

                var existingRecurrence = await _context.TaskRecurrences
                    .Include(tr => tr.TaskRecurrenceDays)
                    .FirstOrDefaultAsync(tr => tr.RecurrenceId == id);

                if (existingRecurrence == null)
                {
                    _logger.LogWarning("Task recurrence with ID {RecurrenceId} not found", id);
                    return NotFound($"Task recurrence with ID {id} not found");
                }

                existingRecurrence.RecurrenceType = recurrenceDto.RecurrenceType;
                existingRecurrence.IntervalDays = recurrenceDto.IntervalDays;
                existingRecurrence.RecurrenceEndDate = recurrenceDto.RecurrenceEndDate;
                existingRecurrence.UpdatedAt = DateTime.UtcNow;
                existingRecurrence.UpdatedBy = 1;

                await _context.SaveChangesAsync();

                var responseDto = new TaskRecurrenceResponseDto
                {
                    RecurrenceId = existingRecurrence.RecurrenceId,
                    RecurrenceType = existingRecurrence.RecurrenceType,
                    IntervalDays = existingRecurrence.IntervalDays,
                    RecurrenceEndDate = existingRecurrence.RecurrenceEndDate,
                    CreatedAt = existingRecurrence.CreatedAt,
                    TaskRecurrenceDays = existingRecurrence.TaskRecurrenceDays.Select(trd => new TaskRecurrenceDayResponseDto
                    {
                        RecurrenceDayId = trd.RecurrenceDayId,
                        RecurrenceId = trd.RecurrenceId,
                        DayOfWeek = trd.DayOfWeek,
                        WeekNumber = trd.WeekNumber
                    }).ToList()
                };

                return Ok(responseDto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating task recurrence with ID {RecurrenceId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The task recurrence was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating task recurrence");
                return StatusCode(500, "Database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating task recurrence");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a task recurrence
        /// </summary>
        /// <param name="id">Task recurrence ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTaskRecurrence(int id)
        {
            try
            {
                var recurrence = await _context.TaskRecurrences.FindAsync(id);

                if (recurrence == null)
                {
                    _logger.LogWarning("Task recurrence with ID {RecurrenceId} not found", id);
                    return NotFound($"Task recurrence with ID {id} not found");
                }

                _context.TaskRecurrences.Remove(recurrence);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting task recurrence with ID {RecurrenceId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task recurrence with ID {RecurrenceId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the task recurrence");
            }
        }
    }
}
