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
    public class UserControllerTests
    {
        private readonly TaskManagerDbContext _context;
        private readonly Mock<ILogger<UserController>> _loggerMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _loggerMock = new Mock<ILogger<UserController>>();
            _controller = new UserController(_context, _loggerMock.Object);
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
                UserType1 = "Admin",
                CreateDate = DateTime.UtcNow
            };

            var organization = new Organization
            {
                OrganizationId = 1,
                OrganizationName = "Test Org",
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var users = new List<User>
            {
                new User
                {
                    UserId = 1,
                    UserName = "john.doe",
                    Email = "john@example.com",
                    UserTypeId = 1,
                    OrganizationId = 1,
                    TimeZoneId = "UTC",
                    IsActive = true,
                    CreateDate = DateTime.UtcNow.AddDays(-10),
                    UpdateDate = DateTime.UtcNow
                },
                new User
                {
                    UserId = 2,
                    UserName = "jane.smith",
                    Email = "jane@example.com",
                    UserTypeId = 1,
                    OrganizationId = 1,
                    TimeZoneId = "UTC",
                    IsActive = true,
                    CreateDate = DateTime.UtcNow.AddDays(-5),
                    UpdateDate = DateTime.UtcNow
                },
                new User
                {
                    UserId = 3,
                    UserName = "bob.jones",
                    Email = "bob@example.com",
                    UserTypeId = 1,
                    OrganizationId = null,
                    TimeZoneId = "UTC",
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                }
            };

            _context.UserTypes.Add(userType);
            _context.Organizations.Add(organization);
            _context.Users.AddRange(users);
            _context.SaveChanges();
        }

        #region GetUsers Tests

        [Fact]
        public async Task GetUsers_WithValidPagination_ReturnsOkWithData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUsers(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PaginatedResponseDto<UserResponseDto>;
            response.Should().NotBeNull();
            response!.Total.Should().Be(3);
            response.Data.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetUsers_FilteredByOrganizationId_ReturnsOkWithFilteredData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUsers(pageNumber: 1, pageSize: 10, organizationId: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<UserResponseDto>;

            response.Should().NotBeNull();
            response!.Total.Should().Be(2);
            response.Data.Should().HaveCount(2);
            response.Data.Should().AllSatisfy(u => u.OrganizationId.Should().Be(1));
        }

        [Fact]
        public async Task GetUsers_WhenEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetUsers(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponseDto<UserResponseDto>;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
            response.Total.Should().Be(0);
        }

        #endregion

        #region GetUser Tests

        [Fact]
        public async Task GetUser_WithValidId_ReturnsOkWithUser()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUser(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var user = okResult.Value as UserResponseDto;
            user.Should().NotBeNull();
            user!.UserId.Should().Be(1);
            user.UserName.Should().Be("john.doe");
            user.Email.Should().Be("john@example.com");
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUser(id: 999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetUser_IncludesOrganizationData()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.GetUser(id: 1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            
            var user = okResult!.Value as UserResponseDto;
            user.Should().NotBeNull();
            user!.UserId.Should().Be(1);
            user.Organization.Should().NotBeNull("User with OrganizationId=1 should have Organization data loaded");
            user.Organization!.OrganizationName.Should().Be("Test Org");
            user.UserType.Should().NotBeNull("UserType should be loaded");
        }

        #endregion

        #region CreateUser Tests

        [Fact]
        public async Task CreateUser_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateUserDto
            {
                UserName = "new.user",
                Email = "new@example.com",
                UserTypeId = 1,
                OrganizationId = 1,
                TimeZoneId = "UTC"
            };

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var user = createdResult.Value as User;
            user.Should().NotBeNull();
            user!.UserName.Should().Be("new.user");
            user.Email.Should().Be("new@example.com");
        }

        [Fact]
        public async Task CreateUser_WithEmptyEmail_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                UserName = "test.user",
                Email = "",
                UserTypeId = 1,
                TimeZoneId = "UTC"
            };

            ValidateModel(dto);

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateUser_SavesUserToDatabase()
        {
            // Arrange
            SeedTestData();
            var dto = new CreateUserDto
            {
                UserName = "database.user",
                Email = "db@example.com",
                UserTypeId = 1,
                TimeZoneId = "UTC"
            };

            // Act
            await _controller.CreateUser(dto);

            // Assert
            var savedUser = _context.Users.FirstOrDefault(u => u.UserName == "database.user");
            savedUser.Should().NotBeNull();
            savedUser!.IsActive.Should().BeTrue();
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_WithValidData_ReturnsOkWithUpdatedUser()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateUserDto
            {
                UserId = 1,
                UserName = "john.updated",
                Email = "john.updated@example.com",
                UserTypeId = 1,
                OrganizationId = 1,
                TimeZoneId = "UTC",
                IsActive = true
            };

            // Act
            var result = await _controller.UpdateUser(id: 1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var user = okResult.Value as User;
            user.Should().NotBeNull();
            user!.UserName.Should().Be("john.updated");
        }

        [Fact]
        public async Task UpdateUser_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateUserDto
            {
                UserId = 2,
                UserName = "test",
                Email = "test@example.com",
                UserTypeId = 1,
                TimeZoneId = "UTC",
                IsActive = true
            };

            // Act
            var result = await _controller.UpdateUser(id: 1, dto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateUser_DeactivatesUser()
        {
            // Arrange
            SeedTestData();
            var dto = new UpdateUserDto
            {
                UserId = 1,
                UserName = "john.doe",
                Email = "john@example.com",
                UserTypeId = 1,
                TimeZoneId = "UTC",
                IsActive = false
            };

            // Act
            await _controller.UpdateUser(id: 1, dto);

            // Assert
            var user = _context.Users.Find(1);
            user.Should().NotBeNull();
            user!.IsActive.Should().BeFalse();
        }

        #endregion

        #region DeleteUser Tests

        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _controller.DeleteUser(id: 1);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteUser_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteUser(id: 999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteUser_SetsIsActiveFalse()
        {
            // Arrange
            SeedTestData();

            // Act
            await _controller.DeleteUser(id: 1);

            // Assert
            var user = _context.Users.Find(1);
            user.Should().NotBeNull();
            user!.IsActive.Should().BeFalse();
        }

        #endregion
    }
}
