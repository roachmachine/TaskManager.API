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
    public class OrganizationController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(TaskManagerDbContext context, ILogger<OrganizationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all organizations
        /// </summary>
        /// <returns>List of organizations</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<OrganizationResponseDto>>> GetOrganizations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Organizations.Where(o => !o.IsDeleted);
                var total = await query.CountAsync();
                var organizations = await query
                    .OrderBy(o => o.OrganizationName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var organizationDtos = organizations.Select(o => new OrganizationResponseDto
                {
                    OrganizationId = o.OrganizationId,
                    OrganizationName = o.OrganizationName,
                    ImageUrl = o.ImageUrl,
                    IsDeleted = o.IsDeleted,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                }).ToList();

                var response = new PaginatedResponseDto<OrganizationResponseDto>
                {
                    Data = organizationDtos,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organizations");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving organizations");
            }
        }

        /// <summary>
        /// Get organization by ID
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <returns>Organization details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]       
        public async Task<ActionResult<OrganizationResponseDto>> GetOrganization(int id, bool includePrograms)
        {
            try
            {
                IQueryable<Organization> query = _context.Organizations;

                if (includePrograms)
                {
                    query = query.Include(o => o.OrgPrograms);
                }

                var organization = await query
                    .FirstOrDefaultAsync(o => o.OrganizationId == id && !o.IsDeleted);

                if (organization == null)
                {
                    _logger.LogWarning("Organization with ID {OrganizationId} not found", id);
                    return NotFound($"Organization with ID {id} not found");
                }

                var dto = new OrganizationResponseDto
                {
                    OrganizationId = organization.OrganizationId,
                    OrganizationName = organization.OrganizationName,
                    ImageUrl = organization.ImageUrl,
                    IsDeleted = organization.IsDeleted,
                    CreatedAt = organization.CreatedAt,
                    UpdatedAt = organization.UpdatedAt,
                };

                if (includePrograms)
                {
                    dto.OrgPrograms = organization.OrgPrograms.Select(p => new OrgProgramResponseDto
                    {
                        OrgProgramId = p.OrgProgramId,
                        ProgramName = p.ProgramName,
                        IsDeleted = p.IsDeleted,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    }).ToList();
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organization with ID {OrganizationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the organization");
            }
        }

        /// <summary>
        /// Create a new organization
        /// </summary>
        /// <param name="organizationDto">Organization data</param>
        /// <returns>Created organization</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrganizationResponseDto>> CreateOrganization([FromBody] CreateOrganizationDto organizationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var organization = new Organization
                {
                    OrganizationName = organizationDto.OrganizationName,
                    ImageUrl = organizationDto.ImageUrl,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = 1,
                    UpdatedBy = 1
                };

                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync();

                var responseDto = new OrganizationResponseDto
                {
                    OrganizationId = organization.OrganizationId,
                    OrganizationName = organization.OrganizationName,
                    ImageUrl = organization.ImageUrl,
                    IsDeleted = organization.IsDeleted,
                    CreatedAt = organization.CreatedAt,
                    UpdatedAt = organization.UpdatedAt
                };

                return CreatedAtAction(nameof(GetOrganization), new { id = organization.OrganizationId }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the organization");
            }
        }

        /// <summary>
        /// Update an existing organization
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <param name="organizationDto">Updated organization data</param>
        /// <returns>Updated organization</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrganizationResponseDto>> UpdateOrganization(int id, [FromBody] UpdateOrganizationDto organizationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != organizationDto.OrganizationId)
                {
                    return BadRequest("Organization ID mismatch");
                }

                var existingOrganization = await _context.Organizations.FindAsync(id);

                if (existingOrganization == null || existingOrganization.IsDeleted)
                {
                    _logger.LogWarning("Organization with ID {OrganizationId} not found", id);
                    return NotFound($"Organization with ID {id} not found");
                }

                existingOrganization.OrganizationName = organizationDto.OrganizationName;
                existingOrganization.ImageUrl = organizationDto.ImageUrl;
                existingOrganization.IsDeleted = organizationDto.IsDeleted;
                existingOrganization.UpdatedAt = DateTime.UtcNow;
                existingOrganization.UpdatedBy = 1;

                await _context.SaveChangesAsync();

                var responseDto = new OrganizationResponseDto
                {
                    OrganizationId = existingOrganization.OrganizationId,
                    OrganizationName = existingOrganization.OrganizationName,
                    ImageUrl = existingOrganization.ImageUrl,
                    IsDeleted = existingOrganization.IsDeleted,
                    CreatedAt = existingOrganization.CreatedAt,
                    UpdatedAt = existingOrganization.UpdatedAt
                };

                return Ok(responseDto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating organization with ID {OrganizationId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The organization was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating organization");
                return StatusCode(500, "Database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating organization");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete an organization (soft delete - marks as deleted)
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(id);

                if (organization == null || organization.IsDeleted)
                {
                    _logger.LogWarning("Organization with ID {OrganizationId} not found", id);
                    return NotFound($"Organization with ID {id} not found");
                }

                organization.IsDeleted = true;
                organization.UpdatedAt = DateTime.UtcNow;
                organization.UpdatedBy = 1;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting organization with ID {OrganizationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organization with ID {OrganizationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the organization");
            }
        }
    }
}
