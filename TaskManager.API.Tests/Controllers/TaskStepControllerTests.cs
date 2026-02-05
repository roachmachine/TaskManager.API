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
    public class TaskStepControllerTests
    {
        private readonly TaskManagerDbContext _context;
        private readonly Mock<ILogger<TaskStepController>> _loggerMock;
        private readonly TaskStepController _controller;

        public TaskStepControllerTests()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _loggerMock = new Mock<ILogger<TaskStepController>>();
            _controller = new TaskStepController(_context, _loggerMock.Object);
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

            var task = new UserTask
            {
                UserTaskId = 1,
                TaskName = "Main Task",
                LocalTime = new TimeOnly(9, 0),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var steps = new List<TaskStep>
            {
                new TaskStep
                {
                    TaskStepId = 1,
                    UserTaskId = 1,
                    StepTitle = "Step 1",
                    StepDescription = "First step",
                    StepOrder = 1,
                    IsCompleted = false,
                    CreatedDate = DateTime.UtcNow.AddDays(-2),
                    UpdateDate = DateTime.UtcNow
                },
                new TaskStep
                {
                    TaskStepId = 2,
                    UserTaskId = 1,
                    StepTitle = "Step 2",
                    StepDescription = "Second step",
                    StepOrder = 2,
                    IsCompleted = false,
                    CreatedDate = DateTime.UtcNow.AddDays(-1),
                    UpdateDate = DateTime.UtcNow
                },
                new TaskStep
                {
                    TaskStepId = 3,
                    UserTaskId = 1,
                    StepTitle = "Step 3",
                    StepDescription = "Third step",
                    StepOrder = 3,
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                }
            };

            _context.UserTypes.Add(userType);
            _context.Users.Add(user);
            _context.UserTasks.Add(task);
            _context.TaskSteps.AddRange(steps);
            _context.SaveChanges();
        }

        #region GetTaskSteps Tests

        [Fact]
        public async Task GetTaskSteps_WithValidPagination_ReturnsOkWithData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskSteps(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PaginatedResponseDto<TaskStepResponseDto>;
            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetTaskSteps_FilteredByUserTaskId_ReturnsOkWithFilteredData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskSteps(pageNumber: 1, pageSize: 10, userTaskId: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<TaskStepResponseDto>;

            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
            response.Data.Should().AllSatisfy(s => s.UserTaskId.Should().Be(1));
        }

        [Fact]
        public async Task GetTaskSteps_WhenEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetTaskSteps(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<TaskStepResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
        }

        #endregion

        #region GetTaskStep Tests

        [Fact]
        public async Task GetTaskStep_WithValidId_ReturnsOkWithStep()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskStep(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var step = okResult.Value as TaskStepResponseDto;
            step.Should().NotBeNull();
            step!.TaskStepId.Should().Be(1);
            step.StepTitle.Should().Be("Step 1");
            step.StepOrder.Should().Be(1);
        }

        [Fact]
        public async Task GetTaskStep_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetTaskStep(id: 999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
        }

        #endregion

        #region CreateTaskStep Tests

        [Fact]
        public async Task CreateTaskStep_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateTaskStepDto
            {
                UserTaskId = 1,
                StepTitle = "New Step",
                StepDescription = "New step description",
                StepOrder = 4
            };

            // Act
            var result = await _controller.CreateTaskStep(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var step = createdResult.Value as TaskStep;
            step.Should().NotBeNull();
            step!.StepTitle.Should().Be("New Step");
            step.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task CreateTaskStep_WithEmptyStepTitle_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateTaskStepDto
            {
                UserTaskId = 1,
                StepTitle = "",
                StepOrder = 1
            };

            // Manually trigger model validation using DataAnnotations
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, validationContext, validationResults, true);
            
            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    _controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? "Validation error");
                }
            }

            // Act
            var result = await _controller.CreateTaskStep(dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTaskStep_SavesStepToDatabase()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateTaskStepDto
            {
                UserTaskId = 1,
                StepTitle = "Database Step",
                StepDescription = "Step for database test",
                StepOrder = 4
            };

            // Act
            await _controller.CreateTaskStep(dto);

            // Assert
            var savedStep = _context.TaskSteps.FirstOrDefault(s => s.StepTitle == "Database Step");
            savedStep.Should().NotBeNull();
            savedStep!.IsCompleted.Should().BeFalse();
        }

        #endregion

        #region UpdateTaskStep Tests

        [Fact]
        public async Task UpdateTaskStep_WithValidData_ReturnsOkWithUpdatedStep()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskStepDto
            {
                TaskStepId = 1,
                UserTaskId = 1,
                StepTitle = "Updated Step 1",
                StepDescription = "Updated description",
                StepOrder = 1,
                IsCompleted = true
            };

            // Act
            var result = await _controller.UpdateTaskStep(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var step = okResult.Value as TaskStep;
            step.Should().NotBeNull();
            step!.StepTitle.Should().Be("Updated Step 1");
            step.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateTaskStep_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskStepDto
            {
                TaskStepId = 2,
                UserTaskId = 1,
                StepTitle = "Test",
                StepOrder = 2,
                IsCompleted = false
            };

            // Act
            var result = await _controller.UpdateTaskStep(id: 1, dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateTaskStep_MarksStepAsCompleted()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateTaskStepDto
            {
                TaskStepId = 2,
                UserTaskId = 1,
                StepTitle = "Step 2",
                StepOrder = 2,
                IsCompleted = true
            };

            // Act
            await _controller.UpdateTaskStep(id: 2, dto);

            // Assert
            var step = _context.TaskSteps.Find(2);
            step.Should().NotBeNull();
            step!.IsCompleted.Should().BeTrue();
            step.CompletedDate.Should().NotBeNull();
        }

        #endregion

        #region DeleteTaskStep Tests

        [Fact]
        public async Task DeleteTaskStep_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.DeleteTaskStep(id: 1);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteTaskStep_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteTaskStep(id: 999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteTaskStep_RemovesStepFromDatabase()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteTaskStep(id: 1);

            // Assert
            var step = _context.TaskSteps.Find(1);
            step.Should().BeNull();
        }

        #endregion
    }
}
