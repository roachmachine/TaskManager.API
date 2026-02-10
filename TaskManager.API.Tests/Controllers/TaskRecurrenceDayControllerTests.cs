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
    public class TaskRecurrenceDayControllerTests
    {
        private readonly TaskManagerDbContext _context;
        private readonly Mock<ILogger<TaskRecurrenceDayController>> _loggerMock;
        private readonly TaskRecurrenceDayController _controller;

        public TaskRecurrenceDayControllerTests()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _loggerMock = new Mock<ILogger<TaskRecurrenceDayController>>();
            _controller = new TaskRecurrenceDayController(_context, _loggerMock.Object);
        }

        private void SeedTestData()
        {
            var recurrence = new TaskRecurrence
            {
                RecurrenceId = 1,
                RecurrenceType = "Weekly",
                IntervalDays = 7,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RowVersion = TestDataHelper.DefaultRowVersion,
                CreatedBy = 1,
                UpdatedBy = 1
            };

            var days = new List<TaskRecurrenceDay>
            {
                new TaskRecurrenceDay 
                { 
                    RecurrenceDayId = 1, 
                    RecurrenceId = 1, 
                    DayOfWeek = 1, 
                    WeekNumber = 0,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new TaskRecurrenceDay 
                { 
                    RecurrenceDayId = 2, 
                    RecurrenceId = 1, 
                    DayOfWeek = 3, 
                    WeekNumber = 0,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new TaskRecurrenceDay 
                { 
                    RecurrenceDayId = 3, 
                    RecurrenceId = 1, 
                    DayOfWeek = 5, 
                    WeekNumber = 0,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                }
            };

            _context.TaskRecurrences.Add(recurrence);
            _context.TaskRecurrenceDays.AddRange(days);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetTaskRecurrenceDays_WithValidPagination_ReturnsOkWithData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskRecurrenceDays(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PaginatedResponseDto<TaskRecurrenceDayResponseDto>;
            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetTaskRecurrenceDay_WithValidId_ReturnsOkWithDay()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskRecurrenceDay(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var day = okResult.Value as TaskRecurrenceDayResponseDto;
            day.Should().NotBeNull();
            day!.RecurrenceDayId.Should().Be(1);
            day.DayOfWeek.Should().Be(1);
        }

        [Fact]
        public async Task GetTaskRecurrenceDay_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskRecurrenceDay(id: 999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateTaskRecurrenceDay_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateTaskRecurrenceDayDto
            {
                RecurrenceId = 1,
                DayOfWeek = 2,
                WeekNumber = 0
            };

            // Act
            var result = await _controller.CreateTaskRecurrenceDay(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var day = createdResult.Value as TaskRecurrenceDay;
            day.Should().NotBeNull();
            day!.DayOfWeek.Should().Be(2);
            day.RecurrenceId.Should().Be(1);
        }
    }
}
