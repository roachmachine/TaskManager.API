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
    public class UserTaskControllerTests
    {
        private readonly TaskManagerDbContext _context;
        private readonly Mock<ILogger<UserTaskController>> _loggerMock;
        private readonly UserTaskController _controller;

        public UserTaskControllerTests()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _loggerMock = new Mock<ILogger<UserTaskController>>();
            _controller = new UserTaskController(_context, _loggerMock.Object);
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
            var userType = new UserType
            {
                UserTypeId = 1,
                UserType1 = "User",
                CreatedAt = DateTime.UtcNow,
                RowVersion = TestDataHelper.DefaultRowVersion
            };

            var user = new User
            {
                UserId = 1,
                UserName = "test.user",
                Email = "test@example.com",
                UserTypeId = 1,
                TimeZoneId = "UTC",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RowVersion = TestDataHelper.DefaultRowVersion,
                CreatedBy = 1,
                UpdatedBy = 1
            };

            var recurrence = new TaskRecurrence
            {
                RecurrenceId = 1,
                RecurrenceType = "Daily",
                IntervalDays = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RowVersion = TestDataHelper.DefaultRowVersion,
                CreatedBy = 1,
                UpdatedBy = 1
            };

            var tasks = new List<UserTask>
            {
                new UserTask
                {
                    UserTaskId = 1,
                    TaskName = "Task A",
                    TaskDescription = "Description A",
                    LocalTime = new TimeOnly(9, 0),
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    EndDate = null,
                    UserId = 1,
                    RecurrenceId = 1,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new UserTask
                {
                    UserTaskId = 2,
                    TaskName = "Task B",
                    TaskDescription = "Description B",
                    LocalTime = new TimeOnly(14, 0),
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    EndDate = null,
                    UserId = 1,
                    RecurrenceId = null,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new UserTask
                {
                    UserTaskId = 3,
                    TaskName = "Task C",
                    TaskDescription = "Description C",
                    LocalTime = new TimeOnly(18, 0),
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    EndDate = null,
                    UserId = 1,
                    RecurrenceId = null,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                }
            };

            _context.UserTypes.Add(userType);
            _context.Users.Add(user);
            _context.TaskRecurrences.Add(recurrence);
            _context.UserTasks.AddRange(tasks);
            _context.SaveChanges();
        }

        #region GetUserTasks Tests

        [Fact]
        public async Task GetUserTasks_WithValidPagination_ReturnsOkWithData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUserTasks(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PaginatedResponseDto<UserTaskResponseDto>;
            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
        }

        #endregion

        #region GetUserTask Tests

        [Fact]
        public async Task GetUserTask_WithValidId_ReturnsOkWithTask()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUserTask(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var userTask = okResult.Value as UserTaskResponseDto;
            userTask.Should().NotBeNull();
            userTask!.UserTaskId.Should().Be(1);
            userTask.TaskName.Should().Be("Task A");
        }

        [Fact]
        public async Task GetUserTask_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUserTask(id: 999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        #endregion

        #region CreateUserTask Tests

        [Fact]
        public async Task CreateUserTask_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateUserTaskDto
            {
                TaskName = "New Task",
                TaskDescription = "Description",
                LocalTime = new TimeOnly(10, 0),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = 1
            };

            // Act
            var result = await _controller.CreateUserTask(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);
            
            var userTask = createdResult.Value as UserTaskResponseDto;
            userTask.Should().NotBeNull();
            userTask!.TaskName.Should().Be("New Task");
        }

        #endregion

        #region UpdateUserTask Tests

        [Fact]
        public async Task UpdateUserTask_WithValidData_ReturnsOkWithUpdatedTask()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateUserTaskDto
            {
                UserTaskId = 1,
                TaskName = "Updated Task A",
                LocalTime = new TimeOnly(9, 0),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = 1,
                IsDeleted = false
            };

            // Act
            var result = await _controller.UpdateUserTask(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var userTask = okResult.Value as UserTask;
            userTask.Should().NotBeNull();
            userTask!.TaskName.Should().Be("Updated Task A");
        }

        [Fact]
        public async Task UpdateUserTask_DeactivatesTask()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateUserTaskDto
            {
                UserTaskId = 1,
                TaskName = "Task A",
                LocalTime = new TimeOnly(9, 0),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = 1,
                IsDeleted = true
            };

            // Act
            await _controller.UpdateUserTask(id: 1, dto);

            // Assert
            var task = _context.UserTasks.Find(1);
            task.Should().NotBeNull();
            task!.IsDeleted.Should().BeTrue();
        }

        #endregion

        #region DeleteUserTask Tests

        [Fact]
        public async Task DeleteUserTask_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.DeleteUserTask(id: 1);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteUserTask_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteUserTask(id: 999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteUserTask_SetsIsDeletedTrue()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteUserTask(id: 1);

            // Assert
            var task = _context.UserTasks.Find(1);
            task.Should().NotBeNull();
            task!.IsDeleted.Should().BeTrue();
        }

        #endregion
    }
}
