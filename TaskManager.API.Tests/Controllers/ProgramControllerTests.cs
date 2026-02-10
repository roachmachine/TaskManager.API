using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using TaskManager.API.Controllers;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Models;

namespace TaskManager.API.Tests.Controllers
{
    public class ProgramControllerTests
    {
        private readonly TaskManagerDbContext _context;
        private readonly Mock<ILogger<ProgramController>> _loggerMock;
        private readonly ProgramController _controller;

        public ProgramControllerTests()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _loggerMock = new Mock<ILogger<ProgramController>>();
            _controller = new ProgramController(_context, _loggerMock.Object);
        }

        private void SeedTestData()
        {
            var org1 = new Organization
            {
                OrganizationId = 1,
                OrganizationName = "Tech Corp",
                IsDeleted = false,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var org2 = new Organization
            {
                OrganizationId = 2,
                OrganizationName = "Global Inc",
                IsDeleted = false,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var programs = new List<ProgramModel>
            {
                new ProgramModel
                {
                    ProgramId = 1,
                    ProgramName = "Program A",
                    OrganizationId = 1,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow.AddDays(-20),
                    UpdateDate = DateTime.UtcNow
                },
                new ProgramModel
                {
                    ProgramId = 2,
                    ProgramName = "Program B",
                    OrganizationId = 1,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow.AddDays(-10),
                    UpdateDate = DateTime.UtcNow
                },
                new ProgramModel
                {
                    ProgramId = 3,
                    ProgramName = "Program C",
                    OrganizationId = 2,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                }
            };

            _context.Organizations.AddRange(org1, org2);
            _context.Programs.AddRange(programs);
            _context.SaveChanges();
        }

        #region GetPrograms Tests

        [Fact]
        public async Task GetPrograms_WithValidPagination_ReturnsOkWithData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetPrograms(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PaginatedResponseDto<ProgramResponseDto>;
            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetPrograms_FilteredByOrganizationId_ReturnsOkWithFilteredData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetPrograms(pageNumber: 1, pageSize: 10, organizationId: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<ProgramResponseDto>;

            response.Should().NotBeNull();
            response!.Total.Should().Be(2);
            response.Data.Should().HaveCount(2);
            response.Data.Should().AllSatisfy(p => p.OrganizationId.Should().Be(1));
        }

        [Fact]
        public async Task GetPrograms_WhenEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetPrograms(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<ProgramResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
            response.Total.Should().Be(0);
        }

        #endregion

        #region GetProgram Tests

        [Fact]
        public async Task GetProgram_WithValidId_ReturnsOkWithProgram()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetProgram(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var program = okResult.Value as ProgramResponseDto;
            program.Should().NotBeNull();
            program!.ProgramId.Should().Be(1);
            program.ProgramName.Should().Be("Program A");
        }

        [Fact]
        public async Task GetProgram_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetProgram(id: 999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetProgram_IncludesOrganizationData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetProgram(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var program = okResult!.Value as ProgramResponseDto;

            program.Should().NotBeNull();
            program!.Organization.Should().NotBeNull();
            program.Organization!.OrganizationName.Should().Be("Tech Corp");
        }

        #endregion

        #region CreateProgram Tests

        [Fact]
        public async Task CreateProgram_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateProgramDto
            {
                ProgramName = "New Program",
                OrganizationId = 1
            };

            // Act
            var result = await _controller.CreateProgram(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var program = createdResult.Value as ProgramModel;
            program.Should().NotBeNull();
            program!.ProgramName.Should().Be("New Program");
            program.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateProgram_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateProgramDto
            {
                ProgramName = "",
                OrganizationId = 1
            };

            // Manually trigger model validation
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(dto, validationContext, validationResults, true);
            
            foreach (var validationResult in validationResults)
            {
                _controller.ModelState.AddModelError(
                    validationResult.MemberNames.FirstOrDefault() ?? string.Empty,
                    validationResult.ErrorMessage ?? string.Empty);
            }

            // Act
            var result = await _controller.CreateProgram(dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateProgram_SavesProgramToDatabase()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateProgramDto
            {
                ProgramName = "Database Program",
                OrganizationId = 1
            };

            // Act
            await _controller.CreateProgram(dto);

            // Assert
            var savedProgram = _context.Programs
                .FirstOrDefault(p => p.ProgramName == "Database Program");
            
            savedProgram.Should().NotBeNull();
            savedProgram!.IsActive.Should().BeTrue();
        }

        #endregion

        #region UpdateProgram Tests

        [Fact]
        public async Task UpdateProgram_WithValidData_ReturnsOkWithUpdatedProgram()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateProgramDto
            {
                ProgramId = 1,
                ProgramName = "Updated Program A",
                OrganizationId = 1,
                IsActive = true
            };

            // Act
            var result = await _controller.UpdateProgram(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var program = okResult.Value as ProgramModel;
            program.Should().NotBeNull();
            program!.ProgramName.Should().Be("Updated Program A");
        }

        [Fact]
        public async Task UpdateProgram_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateProgramDto
            {
                ProgramId = 2,
                ProgramName = "Test",
                OrganizationId = 1,
                IsActive = true
            };

            // Act
            var result = await _controller.UpdateProgram(id: 1, dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateProgram_DeactivatesProgram()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateProgramDto
            {
                ProgramId = 1,
                ProgramName = "Program A",
                OrganizationId = 1,
                IsActive = false
            };

            // Act
            await _controller.UpdateProgram(id: 1, dto);

            // Assert
            var program = _context.Programs.Find(1);
            program.Should().NotBeNull();
            program!.IsActive.Should().BeFalse();
        }

        #endregion

        #region DeleteProgram Tests

        [Fact]
        public async Task DeleteProgram_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.DeleteProgram(id: 1);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteProgram_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteProgram(id: 999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteProgram_SetsIsActiveFalse()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteProgram(id: 1);

            // Assert
            var program = _context.Programs.Find(1);
            program.Should().NotBeNull();
            program!.IsActive.Should().BeFalse();
        }

        #endregion
    }
}
