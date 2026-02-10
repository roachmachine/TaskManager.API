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
using TaskManager.API.Tests.Helpers;

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
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RowVersion = TestDataHelper.DefaultRowVersion,
                CreatedBy = 1,
                UpdatedBy = 1
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
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new TaskNotification
                {
                    TaskNotificationId = 2,
                    RecurrenceId = 1,
                    OffsetValue = 1,
                    OffsetType = "hours",
                    IsEnabled = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new TaskNotification
                {
                    TaskNotificationId = 3,
                    RecurrenceId = 1,
                    OffsetValue = 30,
                    OffsetType = "minutes",
                    IsEnabled = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                }
            };

            _context.TaskRecurrences.Add(recurrence);
            _context.TaskNotifications.AddRange(notifications);
            _context.SaveChanges();
        }

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
        }

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
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateTaskNotification_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateTaskNotificationDto
            {
                RecurrenceId = 1,
                OffsetValue = 30,
                OffsetType = "minutes"
            };

            // Act
            var result = await _controller.CreateTaskNotification(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var notification = createdResult.Value as TaskNotificationResponseDto;
            notification.Should().NotBeNull();
            notification!.OffsetValue.Should().Be(30);
            notification.RecurrenceId.Should().Be(1);
        }
    }
}
