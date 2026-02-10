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
                var query = _context.OrgPrograms.Where(p => !p.IsDeleted);

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
                    ProgramId = p.OrgProgramId,
                    ProgramName = p.ProgramName,
                    OrganizationId = p.OrganizationId,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Organization = p.Organization != null ? new OrganizationResponseDto
                    {
                        OrganizationId = p.Organization.OrganizationId,
                        OrganizationName = p.Organization.OrganizationName,
                        ImageUrl = p.Organization.ImageUrl,
                        IsDeleted = p.Organization.IsDeleted,
                        CreatedAt = p.Organization.CreatedAt,
                        UpdatedAt = p.Organization.UpdatedAt
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
                var program = await _context.OrgPrograms
                    .Include(p => p.Organization)
                    .FirstOrDefaultAsync(p => p.OrgProgramId == id && !p.IsDeleted);

                if (program == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found", id);
                    return NotFound($"Program with ID {id} not found");
                }

                var programDto = new ProgramResponseDto
                {
                    ProgramId = program.OrgProgramId,
                    ProgramName = program.ProgramName,
                    OrganizationId = program.OrganizationId,
                    IsDeleted = program.IsDeleted,
                    CreatedAt = program.CreatedAt,
                    UpdatedAt = program.UpdatedAt,
                    Organization = program.Organization != null ? new OrganizationResponseDto
                    {
                        OrganizationId = program.Organization.OrganizationId,
                        OrganizationName = program.Organization.OrganizationName,
                        ImageUrl = program.Organization.ImageUrl,
                        IsDeleted = program.Organization.IsDeleted,
                        CreatedAt = program.Organization.CreatedAt,
                        UpdatedAt = program.Organization.UpdatedAt
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

                var program = new OrgProgram
                {
                    ProgramName = programDto.ProgramName,
                    OrganizationId = programDto.OrganizationId,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = 1,
                    UpdatedBy = 1
                };

                _context.OrgPrograms.Add(program);
                await _context.SaveChangesAsync();

                var responseDto = new ProgramResponseDto
                {
                    ProgramId = program.OrgProgramId,
                    ProgramName = program.ProgramName,
                    OrganizationId = program.OrganizationId,
                    IsDeleted = program.IsDeleted,
                    CreatedAt = program.CreatedAt,
                    UpdatedAt = program.UpdatedAt
                };

                return CreatedAtAction(nameof(GetProgram), new { id = program.OrgProgramId }, responseDto);
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

                var existingProgram = await _context.OrgPrograms.FindAsync(id);

                if (existingProgram == null || existingProgram.IsDeleted)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found", id);
                    return NotFound($"Program with ID {id} not found");
                }

                existingProgram.ProgramName = programDto.ProgramName;
                existingProgram.OrganizationId = programDto.OrganizationId;
                existingProgram.IsDeleted = programDto.IsDeleted;
                existingProgram.UpdatedAt = DateTime.UtcNow;
                existingProgram.UpdatedBy = 1;

                await _context.SaveChangesAsync();

                var responseDto = new ProgramResponseDto
                {
                    ProgramId = existingProgram.OrgProgramId,
                    ProgramName = existingProgram.ProgramName,
                    OrganizationId = existingProgram.OrganizationId,
                    IsDeleted = existingProgram.IsDeleted,
                    CreatedAt = existingProgram.CreatedAt,
                    UpdatedAt = existingProgram.UpdatedAt
                };

                return Ok(responseDto);
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
        /// Delete a program (soft delete - marks as deleted)
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
                var program = await _context.OrgPrograms.FindAsync(id);

                if (program == null || program.IsDeleted)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found", id);
                    return NotFound($"Program with ID {id} not found");
                }

                program.IsDeleted = true;
                program.UpdatedAt = DateTime.UtcNow;
                program.UpdatedBy = 1;

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
