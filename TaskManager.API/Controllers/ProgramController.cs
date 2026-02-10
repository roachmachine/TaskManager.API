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
    public class ProgramController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<ProgramController> _logger;

        public ProgramController(TaskManagerDbContext context, ILogger<ProgramController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all programs
        /// </summary>
        /// <returns>List of programs</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<ProgramResponseDto>>> GetPrograms(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? organizationId = null)
        {
            try
            {
                var query = _context.Programs.Where(p => p.IsActive);

                if (organizationId.HasValue)
                {
                    query = query.Where(p => p.OrganizationId == organizationId);
                }

                var total = await query.CountAsync();
                var programs = await query
                    .OrderBy(p => p.ProgramName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(p => p.Organization)
                    .ToListAsync();

                var programDtos = programs.Select(p => new ProgramResponseDto
                {
                    ProgramId = p.ProgramId,
                    ProgramName = p.ProgramName,
                    OrganizationId = p.OrganizationId,
                    IsActive = p.IsActive,
                    CreateDate = p.CreateDate,
                    UpdateDate = p.UpdateDate,
                    Organization = p.Organization != null ? new OrganizationDto
                    {
                        OrganizationId = p.Organization.OrganizationId,
                        OrganizationName = p.Organization.OrganizationName,
                        IsDeleted = p.Organization.IsDeleted,
                        CreateDate = p.Organization.CreateDate,
                        UpdateDate = p.Organization.UpdateDate
                    } : null
                }).ToList();

                var response = new PaginatedResponseDto<ProgramResponseDto>
                {
                    Data = programDtos,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving programs");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving programs");
            }
        }

        /// <summary>
        /// Get program by ID
        /// </summary>
        /// <param name="id">Program ID</param>
        /// <returns>Program details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProgramResponseDto>> GetProgram(int id)
        {
            try
            {
                var program = await _context.Programs
                    .Include(p => p.Organization)
                    .FirstOrDefaultAsync(p => p.ProgramId == id);

                if (program == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found", id);
                    return NotFound($"Program with ID {id} not found");
                }

                var programDto = new ProgramResponseDto
                {
                    ProgramId = program.ProgramId,
                    ProgramName = program.ProgramName,
                    OrganizationId = program.OrganizationId,
                    IsActive = program.IsActive,
                    CreateDate = program.CreateDate,
                    UpdateDate = program.UpdateDate,
                    Organization = program.Organization != null ? new OrganizationDto
                    {
                        OrganizationId = program.Organization.OrganizationId,
                        OrganizationName = program.Organization.OrganizationName,
                        IsDeleted = program.Organization.IsDeleted,
                        CreateDate = program.Organization.CreateDate,
                        UpdateDate = program.Organization.UpdateDate
                    } : null
                };

                return Ok(programDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving program with ID {ProgramId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the program");
            }
        }

        /// <summary>
        /// Create a new program
        /// </summary>
        /// <param name="programDto">Program data</param>
        /// <returns>Created program</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProgramResponseDto>> CreateProgram([FromBody] CreateProgramDto programDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var program = new ProgramModel
                {
                    ProgramName = programDto.ProgramName,
                    OrganizationId = programDto.OrganizationId,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                _context.Programs.Add(program);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProgram), new { id = program.ProgramId }, program);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating program");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the program");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating program");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the program");
            }
        }

        /// <summary>
        /// Update an existing program
        /// </summary>
        /// <param name="id">Program ID</param>
        /// <param name="programDto">Updated program data</param>
        /// <returns>Updated program</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProgramResponseDto>> UpdateProgram(int id, [FromBody] UpdateProgramDto programDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != programDto.ProgramId)
                {
                    return BadRequest("Program ID mismatch");
                }

                var existingProgram = await _context.Programs.FindAsync(id);

                if (existingProgram == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found", id);
                    return NotFound($"Program with ID {id} not found");
                }

                existingProgram.ProgramName = programDto.ProgramName;
                existingProgram.OrganizationId = programDto.OrganizationId;
                existingProgram.IsActive = programDto.IsActive;
                existingProgram.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(existingProgram);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating program with ID {ProgramId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The program was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating program");
                return StatusCode(500, "Database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating program");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a program (soft delete - marks as inactive)
        /// </summary>
        /// <param name="id">Program ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProgram(int id)
        {
            try
            {
                var program = await _context.Programs.FindAsync(id);

                if (program == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found", id);
                    return NotFound($"Program with ID {id} not found");
                }

                program.IsActive = false;
                program.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting program with ID {ProgramId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting program with ID {ProgramId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the program");
            }
        }
    }
}
