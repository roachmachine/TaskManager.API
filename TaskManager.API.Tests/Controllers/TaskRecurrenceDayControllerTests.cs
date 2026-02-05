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
                CreatedDate = DateTime.UtcNow
            };

            var days = new List<TaskRecurrenceDay>
            {
                new TaskRecurrenceDay { RecurrenceDayId = 1, RecurrenceId = 1, DayOfWeek = 1, WeekNumber = 0 },
                new TaskRecurrenceDay { RecurrenceDayId = 2, RecurrenceId = 1, DayOfWeek = 3, WeekNumber = 0 },
                new TaskRecurrenceDay { RecurrenceDayId = 3, RecurrenceId = 1, DayOfWeek = 5, WeekNumber = 0 }
            };

            _context.TaskRecurrences.Add(recurrence);
            _context.TaskRecurrenceDays.AddRange(days);
            _context.SaveChanges();
        }

        #region GetTaskRecurrenceDays Tests

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
        public async Task GetTaskRecurrenceDays_FilteredByRecurrenceId_ReturnsOkWithFilteredData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskRecurrenceDays(pageNumber: 1, pageSize: 10, recurrenceId: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<TaskRecurrenceDayResponseDto>;

            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
            response.Data.Should().AllSatisfy(d => d.RecurrenceId.Should().Be(1));
        }

        [Fact]
        public async Task GetTaskRecurrenceDays_WhenEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetTaskRecurrenceDays(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<TaskRecurrenceDayResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
        }

        #endregion

        #region GetTaskRecurrenceDay Tests

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

        #endregion

        #region CreateTaskRecurrenceDay Tests

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
        }

        [Fact]
        public async Task CreateTaskRecurrenceDay_WithInvalidDayOfWeek_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateTaskRecurrenceDayDto
            {
                RecurrenceId = 1,
                DayOfWeek = 7,
                WeekNumber = 0
            };

            // Act
            var result = await _controller.CreateTaskRecurrenceDay(dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTaskRecurrenceDay_SavesDayToDatabase()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateTaskRecurrenceDayDto
            {
                RecurrenceId = 1,
                DayOfWeek = 4,
                WeekNumber = 0
            };

            // Act
            await _controller.CreateTaskRecurrenceDay(dto);

            // Assert
            var savedDay = _context.TaskRecurrenceDays
                .FirstOrDefault(td => td.DayOfWeek == 4 && td.RecurrenceId == 1);
            
            savedDay.Should().NotBeNull();
        }

        #endregion

        #region UpdateTaskRecurrenceDay Tests

        [Fact]
        public async Task UpdateTaskRecurrenceDay_WithValidData_ReturnsOkWithUpdatedDay()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskRecurrenceDayDto
            {
                RecurrenceDayId = 1,
                RecurrenceId = 1,
                DayOfWeek = 2,
                WeekNumber = 0
            };

            // Act
            var result = await _controller.UpdateTaskRecurrenceDay(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var day = okResult.Value as TaskRecurrenceDay;
            day.Should().NotBeNull();
            day!.DayOfWeek.Should().Be(2);
        }

        [Fact]
        public async Task UpdateTaskRecurrenceDay_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskRecurrenceDayDto
            {
                RecurrenceDayId = 2,
                RecurrenceId = 1,
                DayOfWeek = 2,
                WeekNumber = 0
            };

            // Act
            var result = await _controller.UpdateTaskRecurrenceDay(id: 1, dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateTaskRecurrenceDay_UpdatesDayInDatabase()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskRecurrenceDayDto
            {
                RecurrenceDayId = 1,
                RecurrenceId = 1,
                DayOfWeek = 6,
                WeekNumber = 1
            };

            // Act
            await _controller.UpdateTaskRecurrenceDay(id: 1, dto);

            // Assert
            var day = _context.TaskRecurrenceDays.Find(1);
            day.Should().NotBeNull();
            day!.DayOfWeek.Should().Be(6);
            day.WeekNumber.Should().Be(1);
        }

        #endregion

        #region DeleteTaskRecurrenceDay Tests

        [Fact]
        public async Task DeleteTaskRecurrenceDay_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.DeleteTaskRecurrenceDay(id: 1);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteTaskRecurrenceDay_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteTaskRecurrenceDay(id: 999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteTaskRecurrenceDay_RemovesDayFromDatabase()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteTaskRecurrenceDay(id: 1);

            // Assert
            var day = _context.TaskRecurrenceDays.Find(1);
            day.Should().BeNull();
        }

        #endregion
    }
}
