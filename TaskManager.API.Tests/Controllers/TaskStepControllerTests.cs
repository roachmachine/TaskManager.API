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

            var task = new UserTask
            {
                UserTaskId = 1,
                TaskName = "Main Task",
                LocalTime = new TimeOnly(9, 0),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = 1,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RowVersion = TestDataHelper.DefaultRowVersion,
                CreatedBy = 1,
                UpdatedBy = 1
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
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new TaskStep
                {
                    TaskStepId = 2,
                    UserTaskId = 1,
                    StepTitle = "Step 2",
                    StepDescription = "Second step",
                    StepOrder = 2,
                    IsCompleted = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                },
                new TaskStep
                {
                    TaskStepId = 3,
                    UserTaskId = 1,
                    StepTitle = "Step 3",
                    StepDescription = "Third step",
                    StepOrder = 3,
                    IsCompleted = true,
                    IsDeleted = false,
                    CompletedDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RowVersion = TestDataHelper.DefaultRowVersion,
                    CreatedBy = 1,
                    UpdatedBy = 1
                }
            };

            _context.UserTypes.Add(userType);
            _context.Users.Add(user);
            _context.UserTasks.Add(task);
            _context.TaskSteps.AddRange(steps);
            _context.SaveChanges();
        }

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
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateTaskStep_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateTaskStepDto
            {
                UserTaskId = 1,
                StepTitle = "New Step",
                StepDescription = "A new step",
                StepOrder = 4
            };

            // Act
            var result = await _controller.CreateTaskStep(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var step = createdResult.Value as TaskStepResponseDto;
            step.Should().NotBeNull();
            step!.StepTitle.Should().Be("New Step");
            step.StepOrder.Should().Be(4);
        }
    }
}
