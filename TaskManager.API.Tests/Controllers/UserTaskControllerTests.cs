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
                CreateDate = DateTime.UtcNow
            };

            var user = new User
            {
                UserId = 1,
                UserName = "test.user",
                Email = "test@example.com",
                UserTypeId = 1,
                TimeZoneId = "UTC",
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var recurrence = new TaskRecurrence
            {
                RecurrenceId = 1,
                RecurrenceType = "Daily",
                IntervalDays = 1,
                CreatedDate = DateTime.UtcNow
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
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    UpdateDate = DateTime.UtcNow
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
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-3),
                    UpdateDate = DateTime.UtcNow
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
            response!.Total.Should().Be(2);
            response.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUserTasks_FilteredByUserId_ReturnsOkWithFilteredData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUserTasks(pageNumber: 1, pageSize: 10, userId: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<UserTaskResponseDto>;

            response.Should().NotBeNull();
            response!.Total.Should().Be(2);
            response.Data.Should().AllSatisfy(t => t.UserId.Should().Be(1));
        }

        [Fact]
        public async Task GetUserTasks_WhenEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetUserTasks(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<UserTaskResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
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

            var task = okResult.Value as UserTaskResponseDto;
            task.Should().NotBeNull();
            task!.UserTaskId.Should().Be(1);
            task.TaskName.Should().Be("Task A");
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
        }

        [Fact]
        public async Task GetUserTask_IncludesUserAndRecurrenceData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUserTask(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var task = okResult!.Value as UserTaskResponseDto;

            task.Should().NotBeNull();
            task!.User.Should().NotBeNull();
            task.User!.UserName.Should().Be("test.user");
            // Note: Organization and UserType are not included in the nested User DTO
            task.Recurrence.Should().NotBeNull();
            task.Recurrence!.RecurrenceType.Should().Be("Daily");
        }

        [Fact]
        public async Task GetUserTask_IncludesTaskSteps()
        {
            // Arrange
            SeedTestData();
            var step = new TaskStep
            {
                TaskStepId = 1,
                UserTaskId = 1,
                StepTitle = "Step 1",
                StepDescription = "First step",
                StepOrder = 1,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            _context.TaskSteps.Add(step);
            _context.SaveChanges();

            // Act
            var result = await _controller.GetUserTask(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var task = okResult!.Value as UserTaskResponseDto;

            task.Should().NotBeNull();
            task!.TaskSteps.Should().HaveCount(1);
            task.TaskSteps.First().StepTitle.Should().Be("Step 1");
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
                TaskDescription = "New task description",
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

            var task = createdResult.Value as UserTask;
            task.Should().NotBeNull();
            task!.TaskName.Should().Be("New Task");
            task.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateUserTask_WithEmptyTaskName_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateUserTaskDto
            {
                TaskName = "",
                LocalTime = new TimeOnly(10, 0),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = 1
            };

            ValidateModel(dto);

            // Act
            var result = await _controller.CreateUserTask(dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateUserTask_SavesTaskToDatabase()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateUserTaskDto
            {
                TaskName = "Database Task",
                LocalTime = new TimeOnly(15, 30),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = 1
            };

            // Act
            await _controller.CreateUserTask(dto);

            // Assert
            var savedTask = _context.UserTasks.FirstOrDefault(t => t.TaskName == "Database Task");
            savedTask.Should().NotBeNull();
            savedTask!.IsActive.Should().BeTrue();
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
                LocalTime = new TimeOnly(11, 0),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = 1,
                IsActive = true
            };

            // Act
            var result = await _controller.UpdateUserTask(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var task = okResult.Value as UserTask;
            task.Should().NotBeNull();
            task!.TaskName.Should().Be("Updated Task A");
        }

        [Fact]
        public async Task UpdateUserTask_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateUserTaskDto
            {
                UserTaskId = 2,
                TaskName = "Test",
                LocalTime = new TimeOnly(10, 0),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = 1,
                IsActive = true
            };

            // Act
            var result = await _controller.UpdateUserTask(id: 1, dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
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
                IsActive = false
            };

            // Act
            await _controller.UpdateUserTask(id: 1, dto);

            // Assert
            var task = _context.UserTasks.Find(1);
            task.Should().NotBeNull();
            task!.IsActive.Should().BeFalse();
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
        public async Task DeleteUserTask_SetsIsActiveFalse()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteUserTask(id: 1);

            // Assert
            var task = _context.UserTasks.Find(1);
            task.Should().NotBeNull();
            task!.IsActive.Should().BeFalse();
        }

        #endregion
    }
}
