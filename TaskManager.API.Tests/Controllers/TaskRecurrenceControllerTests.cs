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
    public class TaskRecurrenceControllerTests
    {
        private readonly TaskManagerDbContext _context;
        private readonly Mock<ILogger<TaskRecurrenceController>> _loggerMock;
        private readonly TaskRecurrenceController _controller;

        public TaskRecurrenceControllerTests()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _loggerMock = new Mock<ILogger<TaskRecurrenceController>>();
            _controller = new TaskRecurrenceController(_context, _loggerMock.Object);
        }

        private void SeedTestData()
        {
            var recurrences = new List<TaskRecurrence>
            {
                new TaskRecurrence
                {
                    RecurrenceId = 1,
                    RecurrenceType = "Daily",
                    IntervalDays = 1,
                    RecurrenceEndDate = null,
                    CreatedDate = DateTime.UtcNow.AddDays(-20)
                },
                new TaskRecurrence
                {
                    RecurrenceId = 2,
                    RecurrenceType = "Weekly",
                    IntervalDays = 7,
                    RecurrenceEndDate = null,
                    CreatedDate = DateTime.UtcNow.AddDays(-10)
                },
                new TaskRecurrence
                {
                    RecurrenceId = 3,
                    RecurrenceType = "Monthly",
                    IntervalDays = 30,
                    RecurrenceEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
                    CreatedDate = DateTime.UtcNow
                }
            };

            _context.TaskRecurrences.AddRange(recurrences);
            _context.SaveChanges();
        }

        #region GetTaskRecurrences Tests

        [Fact]
        public async Task GetTaskRecurrences_WithValidPagination_ReturnsOkWithData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskRecurrences(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PaginatedResponseDto<TaskRecurrenceResponseDto>;
            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetTaskRecurrences_WithPagination_ReturnsPaginatedData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskRecurrences(pageNumber: 1, pageSize: 2);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<TaskRecurrenceResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().HaveCount(2);
            response.Total.Should().Be(3);
        }

        [Fact]
        public async Task GetTaskRecurrences_WhenEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetTaskRecurrences(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<TaskRecurrenceResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
        }

        #endregion

        #region GetTaskRecurrence Tests

        [Fact]
        public async Task GetTaskRecurrence_WithValidId_ReturnsOkWithRecurrence()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskRecurrence(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var recurrence = okResult.Value as TaskRecurrenceResponseDto;
            recurrence.Should().NotBeNull();
            recurrence!.RecurrenceId.Should().Be(1);
            recurrence.RecurrenceType.Should().Be("Daily");
            recurrence.IntervalDays.Should().Be(1);
        }

        [Fact]
        public async Task GetTaskRecurrence_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskRecurrence(id: 999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        #endregion

        #region CreateTaskRecurrence Tests

        [Fact]
        public async Task CreateTaskRecurrence_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateTaskRecurrenceDto
            {
                RecurrenceType = "Bi-Weekly",
                IntervalDays = 14,
                RecurrenceEndDate = null
            };

            // Act
            var result = await _controller.CreateTaskRecurrence(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var recurrence = createdResult.Value as TaskRecurrence;
            recurrence.Should().NotBeNull();
            recurrence!.RecurrenceType.Should().Be("Bi-Weekly");
            recurrence.IntervalDays.Should().Be(14);
        }

        [Fact]
        public async Task CreateTaskRecurrence_WithZeroIntervalDays_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateTaskRecurrenceDto
            {
                RecurrenceType = "Invalid",
                IntervalDays = 0
            };

            // Manually validate the DTO and add errors to ModelState
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, validationContext, validationResults, true);
            
            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    _controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? string.Empty);
                }
            }

            // Act
            var result = await _controller.CreateTaskRecurrence(dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTaskRecurrence_SavesRecurrenceToDatabase()
        {
            // Arrange
            var dto = new CreateTaskRecurrenceDto
            {
                RecurrenceType = "Quarterly",
                IntervalDays = 90,
                RecurrenceEndDate = null
            };

            // Act
            await _controller.CreateTaskRecurrence(dto);

            // Assert
            var savedRecurrence = _context.TaskRecurrences
                .FirstOrDefault(tr => tr.RecurrenceType == "Quarterly");
            
            savedRecurrence.Should().NotBeNull();
            savedRecurrence!.IntervalDays.Should().Be(90);
        }

        #endregion

        #region UpdateTaskRecurrence Tests

        [Fact]
        public async Task UpdateTaskRecurrence_WithValidData_ReturnsOkWithUpdatedRecurrence()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskRecurrenceDto
            {
                RecurrenceId = 1,
                RecurrenceType = "Hourly",
                IntervalDays = 0
            };

            // Act
            var result = await _controller.UpdateTaskRecurrence(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var recurrence = okResult.Value as TaskRecurrence;
            recurrence.Should().NotBeNull();
            recurrence!.RecurrenceType.Should().Be("Hourly");
        }

        [Fact]
        public async Task UpdateTaskRecurrence_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskRecurrenceDto
            {
                RecurrenceId = 2,
                RecurrenceType = "Test",
                IntervalDays = 1
            };

            // Act
            var result = await _controller.UpdateTaskRecurrence(id: 1, dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateTaskRecurrence_UpdatesRecurrenceInDatabase()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskRecurrenceDto
            {
                RecurrenceId = 1,
                RecurrenceType = "Updated Daily",
                IntervalDays = 1
            };

            // Act
            await _controller.UpdateTaskRecurrence(id: 1, dto);

            // Assert
            var recurrence = _context.TaskRecurrences.Find(1);
            recurrence.Should().NotBeNull();
            recurrence!.RecurrenceType.Should().Be("Updated Daily");
        }

        #endregion

        #region DeleteTaskRecurrence Tests

        [Fact]
        public async Task DeleteTaskRecurrence_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.DeleteTaskRecurrence(id: 1);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteTaskRecurrence_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteTaskRecurrence(id: 999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteTaskRecurrence_RemovesRecurrenceFromDatabase()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteTaskRecurrence(id: 1);

            // Assert
            var recurrence = _context.TaskRecurrences.Find(1);
            recurrence.Should().BeNull();
        }

        #endregion
    }
}
