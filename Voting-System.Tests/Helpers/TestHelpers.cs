using System.Reflection;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem.Entities;
using VotingSystem.Persistence;

namespace VotingSystem.Tests.Helpers
{
    public static class TestHelpers
    {
        public static void ConfigureMapster()
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.Load("Voting-System"));
        }
        public static ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options, Mock.Of<IHttpContextAccessor>());
        }

        public static Application_User CreateTestUser(string id = "test-user-id", string email = "test@test.com")
        {
            return new Application_User
            {
                Id = id,
                Email = email,
                UserName = email,
                FristName = "Test",
                LastName = "User"
            };
        }

        public static ApplicationRole CreateTestRole(string id = "test-role-id", string name = "TestRole")
        {
            return new ApplicationRole
            {
                Id = id,
                Name = name
            };
        }
    }
}

