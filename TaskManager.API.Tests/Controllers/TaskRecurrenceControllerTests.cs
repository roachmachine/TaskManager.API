using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Models;
using TaskManager.API.Tests.Helpers;

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
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new TaskRecurrence
                {
                    RecurrenceId = 2,
                    RecurrenceType = "Weekly",
                    IntervalDays = 7,
                    RecurrenceEndDate = null,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new TaskRecurrence
                {
                    RecurrenceId = 3,
                    RecurrenceType = "Monthly",
                    IntervalDays = 30,
                    RecurrenceEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                }
            };

            _context.TaskRecurrences.AddRange(recurrences);
            _context.SaveChanges();
        }

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

        [Fact]
        public async Task CreateTaskRecurrence_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateTaskRecurrenceDto
            {
                RecurrenceType = "Weekly",
                IntervalDays = 7,
                RecurrenceEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3))
            };

            // Act
            var result = await _controller.CreateTaskRecurrence(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var recurrence = createdResult.Value as TaskRecurrenceResponseDto;
            recurrence.Should().NotBeNull();
            recurrence!.RecurrenceType.Should().Be("Weekly");
            recurrence.IntervalDays.Should().Be(7);
        }
    }
}
