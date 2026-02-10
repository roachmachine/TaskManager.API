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
        /// <remarks>
        /// Sample curl request:
        /// <code>
        /// curl -X GET "https://localhost:7023/api/organization?pageNumber=1&pageSize=10" \
        ///   -H "Authorization: Bearer YOUR_AUTH0_TOKEN"
        /// </code>
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponseDto<Organization>>> GetOrganizations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var total = await _context.Organizations.CountAsync(o => !o.IsDeleted);
                var organizations = await _context.Organizations
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.OrganizationName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var response = new PaginatedResponseDto<Organization>
                {
                    Data = organizations,
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
        /// <remarks>
        /// Sample curl request:
        /// <code>
        /// curl -X GET "https://localhost:7023/api/organization/1" \
        ///   -H "Authorization: Bearer YOUR_AUTH0_TOKEN"
        /// </code>
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]       
        public async Task<ActionResult<Organization>> GetOrganization(
            int id,
            [FromQuery] bool includePrograms = false,
            [FromQuery] bool includeUsers = false)
        {
            try
            {
                var organizationQuery = _context.Organizations.AsQueryable();

                if (includePrograms)
                {
                    organizationQuery = organizationQuery.Include(o => o.Programs);
                }

                if (includeUsers)
                {
                    organizationQuery = organizationQuery.Include(o => o.Users);
                }

                var organization = await organizationQuery
                    .FirstOrDefaultAsync(o => o.OrganizationId == id);

                if (organization == null)
                {
                    _logger.LogWarning("Organization with ID {OrganizationId} not found", id);
                    return NotFound($"Organization with ID {id} not found");
                }

                return Ok(organization);
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
        /// <remarks>
        /// Sample curl request:
        /// <code>
        /// curl -X POST "https://localhost:7023/api/organization" \
        ///   -H "Authorization: Bearer YOUR_AUTH0_TOKEN" \
        ///   -H "Content-Type: application/json" \
        ///   -d '{"organizationName": "Acme Corporation"}'
        /// </code>
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Organization>> CreateOrganization([FromBody] CreateOrganizationDto organizationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(organizationDto.OrganizationName))
                {
                    return BadRequest("Organization name is required");
                }

                var organization = new Organization
                {
                    OrganizationName = organizationDto.OrganizationName,
                    IsDeleted = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOrganization), new { id = organization.OrganizationId }, organization);
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
        /// <remarks>
        /// Sample curl request:
        /// <code>
        /// curl -X PUT "https://localhost:7023/api/organization/1" \
        ///   -H "Authorization: Bearer YOUR_AUTH0_TOKEN" \
        ///   -H "Content-Type: application/json" \
        ///   -d '{"organizationId": 1, "organizationName": "Acme Corp Updated", "isActive": true}'
        /// </code>
        /// </remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Organization>> UpdateOrganization(int id, [FromBody] UpdateOrganizationDto organizationDto)
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

                if (existingOrganization == null)
                {
                    _logger.LogWarning("Organization with ID {OrganizationId} not found", id);
                    return NotFound($"Organization with ID {id} not found");
                }

                // Just modify the tracked entity
                existingOrganization.OrganizationName = organizationDto.OrganizationName;
                existingOrganization.IsDeleted = organizationDto.IsDeleted;
                existingOrganization.UpdateDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(existingOrganization);
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
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete an organization (soft delete - marks as inactive)
        /// </summary>
        /// <param name="id">Organization ID</param>
        /// <returns>No content</returns>
        /// <remarks>
        /// Sample curl request:
        /// <code>
        /// curl -X DELETE "https://localhost:7023/api/organization/1" \
        ///   -H "Authorization: Bearer YOUR_AUTH0_TOKEN"
        /// </code>
        /// </remarks>
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

                if (organization == null)
                {
                    _logger.LogWarning("Organization with ID {OrganizationId} not found", id);
                    return NotFound($"Organization with ID {id} not found");
                }

                organization.IsDeleted = true;
                organization.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error deleting organization with ID {OrganizationId}", id);
                return StatusCode(StatusCodes.Status409Conflict, "The organization was modified by another process");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting organization with ID {OrganizationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database error occurred");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled for organization delete with ID {OrganizationId}", id);
                return StatusCode(StatusCodes.Status408RequestTimeout, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting organization with ID {OrganizationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while deleting the organization");
            }
        }
    }
}
