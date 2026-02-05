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
    public class TaskNotificationControllerTests
    {
        private readonly TaskManagerDbContext _context;
        private readonly Mock<ILogger<TaskNotificationController>> _loggerMock;
        private readonly TaskNotificationController _controller;

        public TaskNotificationControllerTests()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _loggerMock = new Mock<ILogger<TaskNotificationController>>();
            _controller = new TaskNotificationController(_context, _loggerMock.Object);
        }

        private void ValidateModel(object model)
        {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            
            foreach (var validationResult in validationResults)
            {
                _controller.ModelState.AddModelError(
                    validationResult.MemberNames.FirstOrDefault() ?? string.Empty,
                    validationResult.ErrorMessage ?? string.Empty);
            }
        }

        private void SeedTestData()
        {
            var recurrence = new TaskRecurrence
            {
                RecurrenceId = 1,
                RecurrenceType = "Daily",
                IntervalDays = 1,
                CreatedDate = DateTime.UtcNow
            };

            var notifications = new List<TaskNotification>
            {
                new TaskNotification
                {
                    TaskNotificationId = 1,
                    RecurrenceId = 1,
                    OffsetValue = 15,
                    OffsetType = "minutes",
                    IsEnabled = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-10)
                },
                new TaskNotification
                {
                    TaskNotificationId = 2,
                    RecurrenceId = 1,
                    OffsetValue = 1,
                    OffsetType = "hours",
                    IsEnabled = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-5)
                },
                new TaskNotification
                {
                    TaskNotificationId = 3,
                    RecurrenceId = 1,
                    OffsetValue = 30,
                    OffsetType = "minutes",
                    IsEnabled = false,
                    CreatedDate = DateTime.UtcNow
                }
            };

            _context.TaskRecurrences.Add(recurrence);
            _context.TaskNotifications.AddRange(notifications);
            _context.SaveChanges();
        }

        #region GetTaskNotifications Tests

        [Fact]
        public async Task GetTaskNotifications_WithValidPagination_ReturnsOnlyEnabledNotifications()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskNotifications(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PaginatedResponseDto<TaskNotificationResponseDto>;
            response.Should().NotBeNull();
            response!.Total.Should().Be(2);
            response.Data.Should().HaveCount(2);
            response.Data.Should().AllSatisfy(n => n.IsEnabled.Should().BeTrue());
        }

        [Fact]
        public async Task GetTaskNotifications_FilteredByRecurrenceId_ReturnsOkWithFilteredData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskNotifications(pageNumber: 1, pageSize: 10, recurrenceId: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<TaskNotificationResponseDto>;

            response.Should().NotBeNull();
            response!.Total.Should().Be(2);
            response.Data.Should().AllSatisfy(n => n.RecurrenceId.Should().Be(1));
        }

        [Fact]
        public async Task GetTaskNotifications_WhenEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetTaskNotifications(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<TaskNotificationResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
        }

        #endregion

        #region GetTaskNotification Tests

        [Fact]
        public async Task GetTaskNotification_WithValidId_ReturnsOkWithNotification()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskNotification(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var notification = okResult.Value as TaskNotificationResponseDto;
            notification.Should().NotBeNull();
            notification!.TaskNotificationId.Should().Be(1);
            notification.OffsetValue.Should().Be(15);
            notification.OffsetType.Should().Be("minutes");
        }

        [Fact]
        public async Task GetTaskNotification_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskNotification(id: 999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
        }

        #endregion

        #region CreateTaskNotification Tests

        [Fact]
        public async Task CreateTaskNotification_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateTaskNotificationDto
            {
                RecurrenceId = 1,
                OffsetValue = 5,
                OffsetType = "minutes"
            };

            // Act
            var result = await _controller.CreateTaskNotification(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var notification = createdResult.Value as TaskNotification;
            notification.Should().NotBeNull();
            notification!.OffsetValue.Should().Be(5);
            notification.IsEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task CreateTaskNotification_WithEmptyOffsetType_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateTaskNotificationDto
            {
                RecurrenceId = 1,
                OffsetValue = 10,
                OffsetType = ""
            };
            ValidateModel(dto);

            // Act
            var result = await _controller.CreateTaskNotification(dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTaskNotification_SavesNotificationToDatabase()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateTaskNotificationDto
            {
                RecurrenceId = 1,
                OffsetValue = 20,
                OffsetType = "minutes"
            };

            // Act
            await _controller.CreateTaskNotification(dto);

            // Assert
            var savedNotification = _context.TaskNotifications
                .FirstOrDefault(n => n.OffsetValue == 20);
            
            savedNotification.Should().NotBeNull();
            savedNotification!.IsEnabled.Should().BeTrue();
        }

        #endregion

        #region UpdateTaskNotification Tests

        [Fact]
        public async Task UpdateTaskNotification_WithValidData_ReturnsOkWithUpdatedNotification()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskNotificationDto
            {
                TaskNotificationId = 1,
                RecurrenceId = 1,
                OffsetValue = 30,
                OffsetType = "minutes",
                IsEnabled = true
            };

            // Act
            var result = await _controller.UpdateTaskNotification(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var notification = okResult.Value as TaskNotification;
            notification.Should().NotBeNull();
            notification!.OffsetValue.Should().Be(30);
        }

        [Fact]
        public async Task UpdateTaskNotification_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskNotificationDto
            {
                TaskNotificationId = 2,
                RecurrenceId = 1,
                OffsetValue = 15,
                OffsetType = "minutes",
                IsEnabled = true
            };

            // Act
            var result = await _controller.UpdateTaskNotification(id: 1, dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateTaskNotification_DisablesNotification()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskNotificationDto
            {
                TaskNotificationId = 1,
                RecurrenceId = 1,
                OffsetValue = 15,
                OffsetType = "minutes",
                IsEnabled = false
            };

            // Act
            await _controller.UpdateTaskNotification(id: 1, dto);

            // Assert
            var notification = _context.TaskNotifications.Find(1);
            notification.Should().NotBeNull();
            notification!.IsEnabled.Should().BeFalse();
        }

        #endregion

        #region DeleteTaskNotification Tests

        [Fact]
        public async Task DeleteTaskNotification_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.DeleteTaskNotification(id: 1);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteTaskNotification_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteTaskNotification(id: 999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteTaskNotification_SetsIsEnabledFalse()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteTaskNotification(id: 1);

            // Assert
            var notification = _context.TaskNotifications.Find(1);
            notification.Should().NotBeNull();
            notification!.IsEnabled.Should().BeFalse();
        }

        #endregion
    }
}
