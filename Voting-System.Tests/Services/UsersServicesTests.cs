using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Dtos.Common;
using VotingSystem.Dtos.Users;
using VotingSystem.Entities;
using VotingSystem.Errors;
using VotingSystem.Persistence;
using VotingSystem.Repositeryes.Interfaces;
using VotingSystem.Services.Implementation;
using VotingSystem.Services.Interfaces;
using VotingSystem.Tests.Helpers;
using Xunit;
using FluentAssertions;

namespace VotingSystem.Tests.Services
{
    public class UsersServicesTests
    {
        private readonly Mock<UserManager<Application_User>> _userManagerMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleService> _roleServiceMock;
        private readonly ApplicationDbContext _dbContext;
        private readonly UsersServices _usersServices;

        public UsersServicesTests()
        {
            // Configure Mapster for tests
            TestHelpers.ConfigureMapster();

            // Setup InMemory database
            _dbContext = TestHelpers.CreateInMemoryDbContext();

            // Setup UserManager mock
            var userStore = new Mock<IUserStore<Application_User>>();
            _userManagerMock = new Mock<UserManager<Application_User>>(
                userStore.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            // Setup other mocks
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleServiceMock = new Mock<IRoleService>();

            _usersServices = new UsersServices(
                _userManagerMock.Object,
                _dbContext,
                _userRepositoryMock.Object,
                _roleServiceMock.Object);
        }

        [Fact]
        public async Task GetUserDetails_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = "non-existent-user-id";
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((Application_User?)null);

            // Act
            var result = await _usersServices.GetUserDetails(userId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(UserErrors.UserNotFound);
        }

        [Fact]
        public async Task GetUserDetails_UserExists_ReturnsSuccess()
        {
            // Arrange
            var user = TestHelpers.CreateTestUser();
            var roles = new List<string> { "Admin", "Editor" };

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act
            var result = await _usersServices.GetUserDetails(user.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(user.Id);
            result.Value.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task AddUserAsync_EmailAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var request = new AddUserRequest(
                Email: "existing@test.com",
                FristName: "Test",
                LastName: "User",
                UserName: "testuser",
                Password: "Password123!",
                Roles: new List<string>());

            var existingUser = TestHelpers.CreateTestUser(email: request.Email);
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _usersServices.AddUserAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(UserErrors.EmailAlreadyExists);
        }

        [Fact]
        public async Task AddUserAsync_InvalidRoles_ReturnsFailure()
        {
            // Arrange
            var request = new AddUserRequest(
                Email: "new@test.com",
                FristName: "New",
                LastName: "User",
                UserName: "newuser",
                Password: "Password123!",
                Roles: new List<string> { "InvalidRole" });

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((Application_User?)null);
            _userManagerMock.Setup(x => x.FindByNameAsync(request.UserName))
                .ReturnsAsync((Application_User?)null);
            _roleServiceMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Dtos.Roles.RoleResponse>());

            // Act
            var result = await _usersServices.AddUserAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(RoleErrors.InvalidRoles);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var filters = new RequestFilters { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await _usersServices.GetAllUsersAsync(filters);

            // Assert
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.PageNumber.Should().Be(1);
            result.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task ToggleStatus_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = "non-existent-user-id";
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((Application_User?)null);

            // Act
            var result = await _usersServices.ToggleStatus(userId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(UserErrors.UserNotFound);
        }

        [Fact]
        public async Task ToggleStatus_UserExists_TogglesStatus()
        {
            // Arrange
            var user = TestHelpers.CreateTestUser();
            user.IsDisabled = false;

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _usersServices.ToggleStatus(user.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.IsDisabled.Should().BeTrue();
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }
    }
}

