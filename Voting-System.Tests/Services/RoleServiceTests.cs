using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Dtos.Roles;
using VotingSystem.Entities;
using VotingSystem.Errors;
using VotingSystem.Persistence;
using VotingSystem.Services.Implementation;
using Xunit;
using FluentAssertions;

namespace VotingSystem.Tests.Services
{
    public class RoleServiceTests
    {
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<UserManager<Application_User>> _userManagerMock;
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            // Setup InMemory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options, Mock.Of<IHttpContextAccessor>());

            // Setup RoleManager mock
            var roleStore = new Mock<IRoleStore<ApplicationRole>>();
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
                roleStore.Object,
                null!,
                null!,
                null!,
                null!);

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

            _roleService = new RoleService(
                _roleManagerMock.Object,
                _dbContext,
                _userManagerMock.Object);
        }

        [Fact]
        public async Task AssignRoleAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = "non-existent-user-id";
            var request = new AssignRoleRequest { RoleName = "Admin" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((Application_User?)null);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(UserErrors.UserNotFound);
        }

        [Fact]
        public async Task AssignRoleAsync_RoleNotFound_ReturnsFailure()
        {
            // Arrange
            var user = new Application_User { Id = "user-id", Email = "test@test.com" };
            var userId = user.Id;
            var request = new AssignRoleRequest { RoleName = "NonExistentRole" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _roleManagerMock.Setup(x => x.FindByNameAsync("NonExistentRole"))
                .ReturnsAsync((ApplicationRole?)null);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(RoleErrors.RoleNotFound);
        }

        [Fact]
        public async Task AssignRoleAsync_RoleExists_AssignsRoleSuccessfully()
        {
            // Arrange
            var user = new Application_User { Id = "user-id", Email = "test@test.com" };
            var role = new ApplicationRole { Id = "role-id", Name = "Admin" };
            var userId = user.Id;
            var request = new AssignRoleRequest { RoleName = "Admin" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _roleManagerMock.Setup(x => x.FindByNameAsync("Admin"))
                .ReturnsAsync(role);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
        }

        [Fact]
        public async Task AssignRoleAsync_UserAlreadyHasRole_DoesNotAddAgain()
        {
            // Arrange
            var user = new Application_User { Id = "user-id", Email = "test@test.com" };
            var role = new ApplicationRole { Id = "role-id", Name = "Admin" };
            var userId = user.Id;
            var request = new AssignRoleRequest { RoleName = "Admin" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _roleManagerMock.Setup(x => x.FindByNameAsync("Admin"))
                .ReturnsAsync(role);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _roleService.AssignRoleAsync(userId, request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<Application_User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AssignRoleAsync_WithPermissions_ValidPermissions_AssignsSuccessfully()
        {
            // Arrange
            var user = new Application_User { Id = "user-id", Email = "test@test.com" };
            var userId = user.Id;
            var request = new AssignRoleRequest
            {
                Permissions = new List<string> { Permissions.GetPolls, Permissions.AddPolls }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Member" });
            _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IList<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _userManagerMock.Verify(x => x.RemoveFromRolesAsync(user, It.IsAny<IList<string>>()), Times.Once);

            // Verify permissions were added to database
            var userClaims = await _dbContext.UserClaims
                .Where(x => x.UserId == userId && x.ClaimType == Permissions.Type)
                .ToListAsync();
            userClaims.Should().HaveCount(2);
            userClaims.Should().Contain(x => x.ClaimValue == Permissions.GetPolls);
            userClaims.Should().Contain(x => x.ClaimValue == Permissions.AddPolls);
        }

        [Fact]
        public async Task AssignRoleAsync_WithInvalidPermissions_ReturnsFailure()
        {
            // Arrange
            var user = new Application_User { Id = "user-id", Email = "test@test.com" };
            var userId = user.Id;
            var request = new AssignRoleRequest
            {
                Permissions = new List<string> { "invalid.permission" }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(RoleErrors.InvalidPermissions);
        }

        [Fact]
        public async Task AssignRoleAsync_WithRole_RemovesExistingPermissionClaims()
        {
            // Arrange
            var user = new Application_User { Id = "user-id", Email = "test@test.com" };
            var role = new ApplicationRole { Id = "role-id", Name = "Admin" };
            var userId = user.Id;

            // Add existing permission claims
            _dbContext.UserClaims.Add(new IdentityUserClaim<string>
            {
                UserId = userId,
                ClaimType = Permissions.Type,
                ClaimValue = Permissions.GetPolls
            });
            await _dbContext.SaveChangesAsync();

            var request = new AssignRoleRequest { RoleName = "Admin" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _roleManagerMock.Setup(x => x.FindByNameAsync("Admin"))
                .ReturnsAsync(role);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var remainingClaims = await _dbContext.UserClaims
                .Where(x => x.UserId == userId && x.ClaimType == Permissions.Type)
                .ToListAsync();
            remainingClaims.Should().BeEmpty();
        }

        [Fact]
        public async Task AssignRoleAsync_WithPermissions_RemovesExistingRoles()
        {
            // Arrange
            var user = new Application_User { Id = "user-id", Email = "test@test.com" };
            var userId = user.Id;
            var request = new AssignRoleRequest
            {
                Permissions = new List<string> { Permissions.GetPolls }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin", "Editor" });
            _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IList<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _userManagerMock.Verify(
                x => x.RemoveFromRolesAsync(user, It.Is<IList<string>>(roles => roles.Count == 2)),
                Times.Once);
        }
    }
}

