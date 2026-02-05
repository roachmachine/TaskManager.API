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
    public class UserTypeControllerTests
    {
        private readonly TaskManagerDbContext _context;
        private readonly Mock<ILogger<UserTypeController>> _loggerMock;
        private readonly UserTypeController _controller;

        public UserTypeControllerTests()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _loggerMock = new Mock<ILogger<UserTypeController>>();
            _controller = new UserTypeController(_context, _loggerMock.Object);
        }

        private void SeedTestData()
        {
            var userTypes = new List<UserType>
            {
                new UserType
                {
                    UserTypeId = 1,
                    UserType1 = "Admin",
                    CreateDate = DateTime.UtcNow.AddDays(-30)
                },
                new UserType
                {
                    UserTypeId = 2,
                    UserType1 = "Manager",
                    CreateDate = DateTime.UtcNow.AddDays(-20)
                },
                new UserType
                {
                    UserTypeId = 3,
                    UserType1 = "User",
                    CreateDate = DateTime.UtcNow.AddDays(-10)
                }
            };

            _context.UserTypes.AddRange(userTypes);
            _context.SaveChanges();
        }

        #region GetUserTypes Tests

        [Fact]
        public async Task GetUserTypes_WithValidPagination_ReturnsOkWithData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUserTypes(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PaginatedResponseDto<UserTypeResponseDto>;
            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetUserTypes_WithPagination_ReturnsPaginatedData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUserTypes(pageNumber: 1, pageSize: 2);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<UserTypeResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().HaveCount(2);
            response.PageNumber.Should().Be(1);
            response.PageSize.Should().Be(2);
            response.Total.Should().Be(3);
        }

        [Fact]
        public async Task GetUserTypes_WhenEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetUserTypes(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<UserTypeResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
            response.Total.Should().Be(0);
        }

        #endregion

        #region GetUserType Tests

        [Fact]
        public async Task GetUserType_WithValidId_ReturnsOkWithUserType()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUserType(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var userType = okResult.Value as UserTypeResponseDto;
            userType.Should().NotBeNull();
            userType!.UserTypeId.Should().Be(1);
            userType.UserType.Should().Be("Admin");
        }

        [Fact]
        public async Task GetUserType_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUserType(id: 999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        #endregion

        #region CreateUserType Tests

        [Fact]
        public async Task CreateUserType_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateUserTypeDto { UserType = "Supervisor" };

            // Act
            var result = await _controller.CreateUserType(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(UserTypeController.GetUserType));

            var userType = createdResult.Value as UserType;
            userType.Should().NotBeNull();
            userType!.UserType1.Should().Be("Supervisor");
        }

        [Fact]
        public async Task CreateUserType_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateUserTypeDto { UserType = "" };

            // Manually trigger model validation
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(dto, validationContext, validationResults, true);
            
            foreach (var validationResult in validationResults)
            {
                _controller.ModelState.AddModelError(
                    validationResult.MemberNames.FirstOrDefault() ?? string.Empty,
                    validationResult.ErrorMessage ?? string.Empty);
            }

            // Act
            var result = await _controller.CreateUserType(dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CreateUserType_SavesUserTypeToDatabase()
        {
            // Arrange
            var dto = new CreateUserTypeDto { UserType = "Coordinator" };

            // Act
            await _controller.CreateUserType(dto);

            // Assert
            var savedUserType = _context.UserTypes
                .FirstOrDefault(ut => ut.UserType1 == "Coordinator");
            
            savedUserType.Should().NotBeNull();
            savedUserType!.CreateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        #endregion

        #region UpdateUserType Tests

        [Fact]
        public async Task UpdateUserType_WithValidData_ReturnsOkWithUpdatedUserType()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateUserTypeDto
            {
                UserTypeId = 1,
                UserType = "System Admin"
            };

            // Act
            var result = await _controller.UpdateUserType(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var userType = okResult.Value as UserType;
            userType.Should().NotBeNull();
            userType!.UserType1.Should().Be("System Admin");
        }

        [Fact]
        public async Task UpdateUserType_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateUserTypeDto
            {
                UserTypeId = 2,
                UserType = "Updated Name"
            };

            // Act
            var result = await _controller.UpdateUserType(id: 1, dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("User type ID mismatch");
        }

        [Fact]
        public async Task UpdateUserType_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var dto = new UpdateUserTypeDto
            {
                UserTypeId = 999,
                UserType = "Updated Name"
            };

            // Act
            var result = await _controller.UpdateUserType(id: 999, dto);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task UpdateUserType_UpdatesUserTypeInDatabase()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateUserTypeDto
            {
                UserTypeId = 1,
                UserType = "Updated Admin"
            };

            // Act
            await _controller.UpdateUserType(id: 1, dto);

            // Assert
            var userType = _context.UserTypes.Find(1);
            userType.Should().NotBeNull();
            userType!.UserType1.Should().Be("Updated Admin");
        }

        #endregion

        #region DeleteUserType Tests

        [Fact]
        public async Task DeleteUserType_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.DeleteUserType(id: 1);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteUserType_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteUserType(id: 999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DeleteUserType_RemovesUserTypeFromDatabase()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteUserType(id: 1);

            // Assert
            var userType = _context.UserTypes.Find(1);
            userType.Should().BeNull();
        }

        #endregion
    }
}
