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
    public class TaskStepController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<TaskStepController> _logger;

        public TaskStepController(TaskManagerDbContext context, ILogger<TaskStepController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all task steps
        /// </summary>
        /// <returns>List of task steps</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<TaskStepResponseDto>>> GetTaskSteps(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? userTaskId = null)
        {
            try
            {
                var query = _context.TaskSteps.AsQueryable();

                if (userTaskId.HasValue)
                {
                    query = query.Where(ts => ts.UserTaskId == userTaskId);
                }

                var total = await query.CountAsync();
                var steps = await query
                    .OrderBy(ts => ts.StepOrder)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var stepDtos = steps.Select(s => new TaskStepResponseDto
                {
                    TaskStepId = s.TaskStepId,
                    UserTaskId = s.UserTaskId,
                    StepTitle = s.StepTitle,
                    StepDescription = s.StepDescription,
                    StepOrder = s.StepOrder,
                    IsCompleted = s.IsCompleted,
                    CompletedDate = s.CompletedDate,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList();

                var response = new PaginatedResponseDto<TaskStepResponseDto>
                {
                    Data = stepDtos,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task steps");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving task steps");
            }
        }

        /// <summary>
        /// Get task step by ID
        /// </summary>
        /// <param name="id">Task step ID</param>
        /// <returns>Task step details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskStepResponseDto>> GetTaskStep(int id)
        {
            try
            {
                var step = await _context.TaskSteps.FindAsync(id);

                if (step == null)
                {
                    _logger.LogWarning("Task step with ID {TaskStepId} not found", id);
                    return NotFound($"Task step with ID {id} not found");
                }

                var stepDto = new TaskStepResponseDto
                {
                    TaskStepId = step.TaskStepId,
                    UserTaskId = step.UserTaskId,
                    StepTitle = step.StepTitle,
                    StepDescription = step.StepDescription,
                    StepOrder = step.StepOrder,
                    IsCompleted = step.IsCompleted,
                    CompletedDate = step.CompletedDate,
                    CreatedAt = step.CreatedAt,
                    UpdatedAt = step.UpdatedAt
                };

                return Ok(stepDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task step with ID {TaskStepId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the task step");
            }
        }

        /// <summary>
        /// Create a new task step
        /// </summary>
        /// <param name="stepDto">Task step data</param>
        /// <returns>Created task step</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskStepResponseDto>> CreateTaskStep([FromBody] CreateTaskStepDto stepDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var step = new TaskStep
                {
                    UserTaskId = stepDto.UserTaskId,
                    StepTitle = stepDto.StepTitle,
                    StepDescription = stepDto.StepDescription,
                    StepOrder = stepDto.StepOrder,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = 1,
                    UpdatedBy = 1
                };

                _context.TaskSteps.Add(step);
                await _context.SaveChangesAsync();

                var responseDto = new TaskStepResponseDto
                {
                    TaskStepId = step.TaskStepId,
                    UserTaskId = step.UserTaskId,
                    StepTitle = step.StepTitle,
                    StepDescription = step.StepDescription,
                    StepOrder = step.StepOrder,
                    IsCompleted = step.IsCompleted,
                    CompletedDate = step.CompletedDate,
                    CreatedAt = step.CreatedAt,
                    UpdatedAt = step.UpdatedAt
                };

                return CreatedAtAction(nameof(GetTaskStep), new { id = step.TaskStepId }, responseDto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating task step");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the task step");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task step");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the task step");
            }
        }

        /// <summary>
        /// Update an existing task step
        /// </summary>
        /// <param name="id">Task step ID</param>
        /// <param name="stepDto">Updated task step data</param>
        /// <returns>Updated task step</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskStepResponseDto>> UpdateTaskStep(int id, [FromBody] UpdateTaskStepDto stepDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != stepDto.TaskStepId)
                {
                    return BadRequest("Task step ID mismatch");
                }

                var existingStep = await _context.TaskSteps.FindAsync(id);

                if (existingStep == null)
                {
                    _logger.LogWarning("Task step with ID {TaskStepId} not found", id);
                    return NotFound($"Task step with ID {id} not found");
                }

                existingStep.UserTaskId = stepDto.UserTaskId;
                existingStep.StepTitle = stepDto.StepTitle;
                existingStep.StepDescription = stepDto.StepDescription;
                existingStep.StepOrder = stepDto.StepOrder;
                existingStep.IsCompleted = stepDto.IsCompleted;
                existingStep.CompletedDate = stepDto.IsCompleted ? DateTime.UtcNow : null;
                existingStep.UpdatedAt = DateTime.UtcNow;
                existingStep.UpdatedBy = 1;

                await _context.SaveChangesAsync();

                var responseDto = new TaskStepResponseDto
                {
                    TaskStepId = existingStep.TaskStepId,
                    UserTaskId = existingStep.UserTaskId,
                    StepTitle = existingStep.StepTitle,
                    StepDescription = existingStep.StepDescription,
                    StepOrder = existingStep.StepOrder,
                    IsCompleted = existingStep.IsCompleted,
                    CompletedDate = existingStep.CompletedDate,
                    CreatedAt = existingStep.CreatedAt,
                    UpdatedAt = existingStep.UpdatedAt
                };

                return Ok(responseDto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating task step with ID {TaskStepId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The task step was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating task step");
                return StatusCode(500, "Database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating task step");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a task step
        /// </summary>
        /// <param name="id">Task step ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTaskStep(int id)
        {
            try
            {
                var step = await _context.TaskSteps.FindAsync(id);

                if (step == null)
                {
                    _logger.LogWarning("Task step with ID {TaskStepId} not found", id);
                    return NotFound($"Task step with ID {id} not found");
                }

                _context.TaskSteps.Remove(step);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting task step with ID {TaskStepId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task step with ID {TaskStepId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the task step");
            }
        }
    }
}
