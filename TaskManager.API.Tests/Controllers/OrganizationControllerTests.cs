using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Models;

namespace TaskManager.API.Tests.Controllers
{
    public class OrganizationControllerTests
    {
        private readonly TaskManagerDbContext _context;
        private readonly Mock<ILogger<OrganizationController>> _loggerMock;
        private readonly OrganizationController _controller;

        public OrganizationControllerTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _loggerMock = new Mock<ILogger<OrganizationController>>();
            _controller = new OrganizationController(_context, _loggerMock.Object);
        }

        private void SeedTestData()
        {
            var organizations = new List<Organization>
            {
                new Organization
                {
                    OrganizationId = 1,
                    OrganizationName = "Acme Corporation",
                    IsActive = true,
                    CreateDate = DateTime.UtcNow.AddDays(-30),
                    UpdateDate = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationId = 2,
                    OrganizationName = "Tech Solutions Inc",
                    IsActive = true,
                    CreateDate = DateTime.UtcNow.AddDays(-20),
                    UpdateDate = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationId = 3,
                    OrganizationName = "Global Services Ltd",
                    IsActive = true,
                    CreateDate = DateTime.UtcNow.AddDays(-10),
                    UpdateDate = DateTime.UtcNow
                }
            };

            _context.Organizations.AddRange(organizations);
            _context.SaveChanges();
        }

        #region GetOrganizations Tests

        [Fact]
        public async Task GetOrganizations_WithValidPagination_ReturnsOkWithData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetOrganizations(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PaginatedResponseDto<Organization>;
            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetOrganizations_WithPagination_ReturnsPaginatedData()
        {
            // Arrange
            SeedTestData();

            // Act - Get page 1 with size 2
            var result = await _controller.GetOrganizations(pageNumber: 1, pageSize: 2);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<Organization>;

            response.Should().NotBeNull();
            response!.Data.Should().HaveCount(2);
            response.PageNumber.Should().Be(1);
            response.PageSize.Should().Be(2);
            response.Total.Should().Be(3);
        }

        [Fact]
        public async Task GetOrganizations_WithSecondPage_ReturnsCorrectPage()
        {
            // Arrange
            SeedTestData();

            // Act - Get page 2 with size 2
            var result = await _controller.GetOrganizations(pageNumber: 2, pageSize: 2);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<Organization>;

            response.Should().NotBeNull();
            response!.Data.Should().HaveCount(1);
            // Organizations are ordered by name, so page 2 with size 2 should have the 3rd item
            response.Data.First().OrganizationName.Should().Be("Tech Solutions Inc");
        }

        [Fact]
        public async Task GetOrganizations_WhenEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetOrganizations(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<Organization>;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
            response.Total.Should().Be(0);
        }

        #endregion

        #region GetOrganization Tests

        [Fact]
        public async Task GetOrganization_WithValidId_ReturnsOkWithOrganization()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetOrganization(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var organization = okResult.Value as Organization;
            organization.Should().NotBeNull();
            organization!.OrganizationId.Should().Be(1);
            organization.OrganizationName.Should().Be("Acme Corporation");
        }

        [Fact]
        public async Task GetOrganization_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetOrganization(id: 999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetOrganization_WithIncludePrograms_ReturnsOrganizationWithPrograms()
        {
            // Arrange
            SeedTestData();
            var program = new ProgramModel
            {
                ProgramId = 1,
                ProgramName = "Program A",
                OrganizationId = 1,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            _context.Programs.Add(program);
            _context.SaveChanges();

            // Act
            var result = await _controller.GetOrganization(id: 1, includePrograms: true);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var organization = okResult!.Value as Organization;
            
            organization.Should().NotBeNull();
            organization!.Programs.Should().HaveCount(1);
        }

        #endregion

        #region CreateOrganization Tests

        [Fact]
        public async Task CreateOrganization_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateOrganizationDto { OrganizationName = "New Company" };

            // Act
            var result = await _controller.CreateOrganization(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(OrganizationController.GetOrganization));

            var organization = createdResult.Value as Organization;
            organization.Should().NotBeNull();
            organization!.OrganizationName.Should().Be("New Company");
            organization.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateOrganization_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateOrganizationDto { OrganizationName = "" };

            // Act
            var result = await _controller.CreateOrganization(dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CreateOrganization_SavesOrganizationToDatabase()
        {
            // Arrange
            var dto = new CreateOrganizationDto { OrganizationName = "Database Test Company" };

            // Act
            await _controller.CreateOrganization(dto);

            // Assert
            var savedOrganization = _context.Organizations
                .FirstOrDefault(o => o.OrganizationName == "Database Test Company");
            
            savedOrganization.Should().NotBeNull();
            savedOrganization!.IsActive.Should().BeTrue();
            savedOrganization.CreateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        #endregion

        #region UpdateOrganization Tests

        [Fact]
        public async Task UpdateOrganization_WithValidData_ReturnsOkWithUpdatedOrganization()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateOrganizationDto
            {
                OrganizationId = 1,
                OrganizationName = "Updated Acme",
                IsActive = true
            };

            // Act
            var result = await _controller.UpdateOrganization(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var organization = okResult.Value as Organization;
            organization.Should().NotBeNull();
            organization!.OrganizationName.Should().Be("Updated Acme");
        }

        [Fact]
        public async Task UpdateOrganization_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateOrganizationDto
            {
                OrganizationId = 2,
                OrganizationName = "Updated Name",
                IsActive = true
            };

            // Act
            var result = await _controller.UpdateOrganization(id: 1, dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Organization ID mismatch");
        }

        [Fact]
        public async Task UpdateOrganization_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var dto = new UpdateOrganizationDto
            {
                OrganizationId = 999,
                OrganizationName = "Updated Name",
                IsActive = true
            };

            // Act
            var result = await _controller.UpdateOrganization(id: 999, dto);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task UpdateOrganization_DeactivatesOrganization()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateOrganizationDto
            {
                OrganizationId = 1,
                OrganizationName = "Acme Corporation",
                IsActive = false
            };

            // Act
            await _controller.UpdateOrganization(id: 1, dto);

            // Assert
            var organization = _context.Organizations.Find(1);
            organization.Should().NotBeNull();
            organization!.IsActive.Should().BeFalse();
        }

        #endregion

        #region DeleteOrganization Tests

        [Fact]
        public async Task DeleteOrganization_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.DeleteOrganization(id: 1);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteOrganization_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteOrganization(id: 999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DeleteOrganization_SetsIsActiveFalse()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteOrganization(id: 1);

            // Assert
            var organization = _context.Organizations.Find(1);
            organization.Should().NotBeNull();
            organization!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteOrganization_DoesNotHardDelete()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteOrganization(id: 1);

            // Assert
            var organization = _context.Organizations.Find(1);
            organization.Should().NotBeNull(); // Record still exists
        }

        #endregion
    }
}
